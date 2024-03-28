using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Engine.Commands;
using Assets.Scripts.Engine.Moves;
using Assets.Scripts.Engine.Player;
using Assets.Scripts.Visualizer;
using Assets.Scripts.Visualizer.VisualCommands;
using JetBrains.Annotations;
using OpenCover.Framework.Model;
using RSG;
using UnityEngine;
using File = System.IO.File;

namespace Assets.Scripts.Engine
{
    public class Match3Game : IDisposable
    {
        public Match3Field Field { get; private set; }
        public Match3Matcher Matcher { get; private set; }
        public IFieldVisualizer Visuals { get; private set; }
        public PlayersManager PlayersManager { get; private set; }

        public Match3FieldGenerator FieldGenerator { get; private set; }
        public Match3TokenGenerator TokenGenerator { get; private set; }
        
        private Match3Replay Replay { get; set; }
        public Match3MoveProvider MoveProvider { get; private set; }

        public Direction TokensSpawnDirection { get; private set; }

        public Match3GameSettings Settings { get; private set; }

        public bool IsActive { get; private set; } = true;

        public Match3Game(Match3GameSettings settings)
        {
            InitSettings(settings);
            Replay = new Match3Replay {settings = settings, initialField = Field.field.Clone() as Match3Token[,]};
            MoveProvider = new Match3MoveProviderBasic(this);
        }

        private void InitSettings(Match3GameSettings settings)
        {
            Settings = settings;

            TokensSpawnDirection = settings.tokensSpawnDirection;
            Matcher = new BasicMatcher();
            FieldGenerator = new Match3FieldGenerator(Matcher, settings.seed);
            TokenGenerator = new Match3TokenGenerator(settings.seed);
            Field = new Match3Field(settings.width, settings.height, FieldGenerator, Matcher);
        }

        public void ResetField(Match3Token[,] field)
        {
            Field = new Match3Field(field, Matcher);
            Visuals?.SetField(Field);
        }

        public Match3Game(Match3Replay replay)
        {
            InitSettings(replay.settings);
            Field = new Match3Field(replay.initialField, Matcher);
            MoveProvider = new Match3MoveProviderReplay(replay, this);

            var players = replay.players.Select(x => x.Create());
            SetupPlayers(players.ToArray());
        }

        public void SetActive(bool active)
        {
            IsActive = active;
        }

        public void Dispose()
        {
        }

        public void SetupVisuals(FieldVisualUI visual)
        {
            Visuals = visual;
            visual.SetField(Field);
        }

        public void SetupPlayers(params Match3Player[] players)
        {
            PlayersManager = new PlayersManager(Settings, players);
            if (Replay != null)
                Replay.players = players.Select(Match3PlayerData.Save).ToArray();
        }

        public void PlayerMoveInput(Match3CommandMove move)
        {
            if (!IsActive)
                return;

            var player = PlayersManager.CurrentPlayer;
            if (player.CanMakeAMove == false)
                return;

            if (!(player.MoveMaker is HumanMoveMaker humanMoveMaker))
                return;

            var (possible, hasMatches) = move.IsValidAndPossible(Matcher, Field.GetField);
            if (!possible)
                return;

            if (hasMatches)
                humanMoveMaker.MakeAMove(move);
            else
            {
                ShowInvalidMove(move).RunAllSequence(Visuals);
            }
        }

        private FieldVisualCommand ShowInvalidMove(Match3CommandMove move)
        {
            if (!(move is Match3CommandMoveSwap)) 
                throw  new Exception("No invalid animation");

            var start = new FieldVisualSetBlock(true);

            var forward = move.GetVisuals(this);
            var backwards = move.GetVisuals(this);

            start.SetNext(forward, new FieldVisualPause(AnimationTimings.WRONG_MOVE_PAUSE), backwards, new FieldVisualSetBlock(false));
            return start;
        }

        public IPromise MakeMove(Match3Player p, Match3CommandMove move, Match3ReplayTurn replay = null)
        {
            var visual = MakeMoveInnerNew(p, move, out var turnReplay, replay);
            if (replay == null)
                Replay.turns.Add(turnReplay);

            var needVisuals = Visuals != null;
            return needVisuals ? visual.RunAllSequence(Visuals) : Promise.Resolved();
        }

        private FieldVisualCommandSequence MakeMoveInnerNew(Match3Player p, Match3CommandMove move, out Match3ReplayTurn turnReplay, [CanBeNull]Match3ReplayTurn turn = null)
        {
            var needVisuals = Visuals != null;
            var visuals = new FieldVisualCommandSequence();
            var commands = new List<Match3GameCommand>();

            void DoCommand<T>(T cmd, bool loadCheck = true) where T : Match3GameCommand
            {
                if (loadCheck)
                    turn?.AdvanceAndLoadCommand(cmd);

                cmd.Apply(this);
                commands.Add(cmd);
                AddVisuals(cmd.GetVisuals(this));
            }

            void AddVisuals(FieldVisualCommand cmd)
            {
                if (needVisuals)
                    visuals.Add(cmd);
            }

            var speedup = .5f;
            float GetAnimTime() => .5f + speedup;


            AddVisuals(new FieldVisualSetBlock(true));
            DoCommand(move, false);
            AddVisuals(new FieldVisualPause(AnimationTimings.AFTER_MOVE_PAUSE));

            while (true)
            {
                DoCommand(new Match3CommandClearField());
                DoCommand(new Match3CommandTokensSpawnFall(GetAnimTime()));
                if (Matcher.FullCheck(Field.field).Count <= 0)
                    break;

                speedup *= .9f;
            }

            if (!Field.HasAnyMoves()) // TODO tests
                DoCommand(new Match3CommandShuffle(FieldGenerator, Field.Width, Field.Height));

            if (turn != null)
            {
                turnReplay = turn;
            }
            else
            {
                turnReplay = new Match3ReplayTurn();
                foreach (var cmd in commands)
                    turnReplay.AddSaveCommand(cmd);
            }

            AddVisuals(new FieldVisualPause(.5f));
            AddVisuals(new FieldVisualSetBlock(false));
            return visuals;
        }

        /*
        private FieldVisualCommand MakeMoveInner(Match3Player p, Match3CommandMove move, bool needVisuals = true, Match3MoveShuffle[] preRecordedShuffles = null)
        {
            var currentShuffleMove = 0;
            Match3MoveShuffle GetShuffleMove()
            {
                if (!(preRecordedShuffles?.Length > currentShuffleMove))
                    return null;

                var s = preRecordedShuffles[currentShuffleMove];
                currentShuffleMove++;
                return s;
            }
            
            Field.ApplyMove(move, out var moveVisuals);

            var tokens = Field.ClearAfterMove(out var valueField);
            var tokensContainer = new Match3CollectedTokensContainer();
            tokensContainer.AddValue(tokens);

            FieldVisualCommand start = new FieldVisualEmptyNode();
            var lastNode = start;

            if (needVisuals)
            {
                var hideMap = Match3Field.CreateHideMask(valueField);
                // блокируем ввод, двигаем фишки, пауза
                lastNode = lastNode.SetNext(new FieldVisualSetBlock(true), moveVisuals, 
                    new FieldVisualPause(AnimationTimings.AFTER_MOVE_PAUSE));
                // параллельно: 1) чистим фишки 2) пауза + отправляем ресурсы в ЮИ
                lastNode = lastNode.SetNext(new FieldVisualCommandAll(new FieldVisualHide(hideMap),
                    new FieldVisualPause(.25f).SetNext(new FieldVisualSpawnResource(p, valueField))));
            }

            var speedup = .5f;
            float GetAnimTime() => .5f + speedup;

            while (true)
            {
                Field.FallDown(TokensSpawnDirection, out var moveMatrix);
                Field.SpawnNewTokens(TokenGenerator, out var spawned);

                if (needVisuals)
                {
                    var fallDownAnim = new FieldVisualMove(moveMatrix, GetAnimTime());
                    var spawnAndFall = new FieldVisualSpawn(spawned, TokensSpawnDirection, GetAnimTime());

                    lastNode = lastNode.SetNext(new FieldVisualCommandAll(fallDownAnim, spawnAndFall));
                }

                var value = Field.ClearAfterMove(out var valueMask2);
                if (value.Item2.Length == 0)
                    break;

                speedup *= .9f;
                tokensContainer.AddValue(value);

                if (needVisuals)
                {
                    var hideMap = Match3Field.CreateHideMask(valueMask2);
                    lastNode = lastNode.SetNext(new FieldVisualSpawnResource(p, valueMask2));
                    lastNode = lastNode.SetNext(new FieldVisualHide(hideMap, GetAnimTime()));
                }
            }

            if (!Field.HasAnyMoves() || true)
            {
                var shuffle = GetShuffleMove() ?? new Match3MoveShuffle(FieldGenerator, Field.Width, Field.Height);
                Field.ApplyMove(shuffle, out var shuffleVisuals);
                lastNode = lastNode.SetNext(shuffleVisuals);
                lastNode = lastNode.Last;
            }

            if (needVisuals)
                lastNode = lastNode.SetNext(new FieldVisualSetBlock(false));

            PlayersManager.InspectReward(tokensContainer.MatchesCount);
            PlayersManager.CurrentPlayer.Collect(tokensContainer.CollectedCount.Select(x => (x.Key, x.Value)), true, false);
            
            return start;
        }
        */
        public static string replayPath = "G:\\Save.txt";
        public void Save(string path)
        {
            Replay?.Save(path);
        }
    }
}