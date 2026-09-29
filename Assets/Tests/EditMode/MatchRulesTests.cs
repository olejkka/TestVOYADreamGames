using System;
using System.Collections.Generic;
using _Project.Scripts;
using _Project.Scripts.Configs;
using _Project.Scripts.Generators;
using _Project.Scripts.NetworkLayer.Protocol;
using _Project.Scripts.NetworkLayer.Server;
using _Project.Scripts.Ships;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    public class MatchRulesTests
    {
        private readonly List<UnityEngine.Object> _created = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _created.Count; i++)
                UnityEngine.Object.DestroyImmediate(_created[i]);

            _created.Clear();
        }

        [Test]
        public void Place_UsesFleetLengths_AndKeepsShipsApart()
        {
            Match match = CreateMatch(0, 0);
            match.Place();

            AssertFleet(match.SnapshotFor(0));
            AssertFleet(match.SnapshotFor(1));
        }

        [Test]
        public void Shot_ReportsMissHitAndSunk()
        {
            Match match = CreateMatch(0, 0);
            match.Place();

            SnapshotMessage enemy = match.SnapshotFor(1);
            int miss = WaterCell(enemy, new bool[enemy.width * enemy.height]);
            Assert.IsTrue(match.TryShoot(0, miss));
            Assert.AreEqual(ShotResult.Miss, LastShot(match, 0).result);

            var usedByOpponent = new bool[enemy.width * enemy.height];
            int answer = WaterCell(match.SnapshotFor(0), usedByOpponent);
            usedByOpponent[answer] = true;
            match.TryShoot(1, answer);

            ShipPlacement ship = LongShip(match.SnapshotFor(1));
            Assert.IsTrue(match.TryShoot(0, ship.cells[0]));
            Assert.AreEqual(ShotResult.Hit, LastShot(match, 0).result);

            PassTurn(match, 1, usedByOpponent);
            Assert.IsTrue(match.TryShoot(0, ship.cells[1]));
            PassTurn(match, 1, usedByOpponent);
            Assert.IsTrue(match.TryShoot(0, ship.cells[2]));
            Assert.AreEqual(ShotResult.Sunk, LastShot(match, 0).result);
        }

        [Test]
        public void Shot_OutOfTurn_DoesNotApply()
        {
            Match match = CreateMatch(0, 0);
            match.Place();

            int cell = WaterCell(match.SnapshotFor(0), new bool[36]);
            Assert.IsFalse(match.TryShoot(1, cell));

            SnapshotMessage snapshot = match.SnapshotFor(1);
            Assert.AreEqual(0, snapshot.turn);
            Assert.AreEqual(0, snapshot.shots.Length);
        }

        [Test]
        public void Shot_RepeatCell_DoesNotChangeTurn()
        {
            Match match = CreateMatch(0, 0);
            match.Place();

            int cell = WaterCell(match.SnapshotFor(1), new bool[36]);
            Assert.IsTrue(match.TryShoot(0, cell));
            PassTurn(match, 1);

            Assert.AreEqual(0, match.SnapshotFor(0).turn);
            Assert.IsFalse(match.TryShoot(0, cell));
            Assert.AreEqual(0, match.SnapshotFor(0).turn);
            Assert.AreEqual(1, match.SnapshotFor(0).shots.Length);
        }

        [Test]
        public void Hit_DoesNotGrantExtraTurn()
        {
            Match match = CreateMatch(0, 0);
            match.Place();

            ShipPlacement ship = LongShip(match.SnapshotFor(1));
            Assert.IsTrue(match.TryShoot(0, ship.cells[0]));

            SnapshotMessage snapshot = match.SnapshotFor(0);
            Assert.AreEqual(ShotResult.Hit, snapshot.shots[0].result);
            Assert.AreEqual(1, snapshot.turn);
        }

        [Test]
        public void LastShip_MakesShooterTheWinner()
        {
            Match match = CreateMatch(0, 0);
            match.Place();

            ShipPlacement[] ships = match.SnapshotFor(1).ships;
            bool[] usedByOpponent = new bool[36];

            for (int s = 0; s < ships.Length; s++)
            {
                int[] cells = ships[s].cells;
                for (int c = 0; c < cells.Length; c++)
                {
                    if (match.SnapshotFor(0).turn != 0)
                        PassTurn(match, 1, usedByOpponent);

                    Assert.IsTrue(match.TryShoot(0, cells[c]));
                }
            }

            SnapshotMessage snapshot = match.SnapshotFor(0);
            Assert.AreEqual(0, snapshot.winner);
            Assert.AreEqual(0, snapshot.turn);
        }

        [Test]
        public void ExpireTurn_PassesTurnAfterTimeout()
        {
            Match match = CreateMatch(5, 0);
            match.Place();
            match.StartTurn(1000);

            Assert.IsFalse(match.ExpireTurn(5999));
            Assert.AreEqual(0, match.SnapshotFor(0).turn);

            Assert.IsTrue(match.ExpireTurn(6000));
            SnapshotMessage snapshot = match.SnapshotFor(0);
            Assert.AreEqual(1, snapshot.turn);
            Assert.AreEqual(SnapshotMessage.NoWinner, snapshot.winner);
            Assert.AreEqual(11000, snapshot.turnDeadlineMs);
        }

        [Test]
        public void ExpireAbsence_AwardsWinOnlyWhenTheOtherPlayerIsPresent()
        {
            Match bothGone = CreateMatch(0, 5);
            bothGone.Place();
            bothGone.MarkAbsent(0, 0);
            bothGone.MarkAbsent(1, 0);

            Assert.IsFalse(bothGone.ExpireAbsence(5000));
            Assert.AreEqual(SnapshotMessage.NoWinner, bothGone.SnapshotFor(0).winner);

            bothGone.MarkPresent(0);
            Assert.IsTrue(bothGone.ExpireAbsence(5000));
            Assert.AreEqual(0, bothGone.SnapshotFor(0).winner);

            Match oneGone = CreateMatch(0, 5);
            oneGone.Place();
            oneGone.MarkAbsent(1, 1000);

            Assert.IsFalse(oneGone.ExpireAbsence(5999));
            Assert.IsTrue(oneGone.ExpireAbsence(6000));
            Assert.AreEqual(0, oneGone.SnapshotFor(1).winner);
            Assert.AreEqual(SnapshotMessage.NoDeadline, oneGone.SnapshotFor(0).turnDeadlineMs);
        }

        private Match CreateMatch(int turnTimeoutSec, int timeOut)
        {
            var matchConfig = ScriptableObject.CreateInstance<MatchConfig>();
            _created.Add(matchConfig);
            matchConfig.width = 6;
            matchConfig.height = 6;
            matchConfig.turnTimeoutSec = turnTimeoutSec;
            matchConfig.timeOut = timeOut;
            matchConfig.fleet = new[]
            {
                new FleetEntry { Type = ShipType.Long, Count = 1 },
                new FleetEntry { Type = ShipType.Medium, Count = 2 },
                new FleetEntry { Type = ShipType.Short, Count = 1 }
            };

            var lengths = ScriptableObject.CreateInstance<ShipLengthConfig>();
            _created.Add(lengths);
            lengths.entries = new[]
            {
                new ShipLengthEntry { Type = ShipType.Long, Length = 3 },
                new ShipLengthEntry { Type = ShipType.Medium, Length = 2 },
                new ShipLengthEntry { Type = ShipType.Short, Length = 1 }
            };

            return new Match(
                new FieldGenerator(matchConfig),
                new ShipPositionGenerator(matchConfig, lengths),
                matchConfig);
        }

        private static void AssertFleet(SnapshotMessage snapshot)
        {
            Assert.AreEqual(4, snapshot.ships.Length);

            int longCount = 0;
            int mediumCount = 0;
            int shortCount = 0;

            for (int i = 0; i < snapshot.ships.Length; i++)
            {
                ShipPlacement ship = snapshot.ships[i];
                if (ship.type == ShipType.Long)
                {
                    Assert.AreEqual(3, ship.cells.Length);
                    longCount++;
                }
                else if (ship.type == ShipType.Medium)
                {
                    Assert.AreEqual(2, ship.cells.Length);
                    mediumCount++;
                }
                else if (ship.type == ShipType.Short)
                {
                    Assert.AreEqual(1, ship.cells.Length);
                    shortCount++;
                }
            }

            Assert.AreEqual(1, longCount);
            Assert.AreEqual(2, mediumCount);
            Assert.AreEqual(1, shortCount);

            for (int a = 0; a < snapshot.ships.Length; a++)
            {
                for (int b = a + 1; b < snapshot.ships.Length; b++)
                {
                    int[] left = snapshot.ships[a].cells;
                    int[] right = snapshot.ships[b].cells;
                    for (int i = 0; i < left.Length; i++)
                    {
                        for (int j = 0; j < right.Length; j++)
                            Assert.IsFalse(TooClose(left[i], right[j], snapshot.width));
                    }
                }
            }
        }

        private static bool TooClose(int a, int b, int width)
        {
            int dx = Math.Abs(a % width - b % width);
            int dy = Math.Abs(a / width - b / width);
            return Math.Max(dx, dy) <= 1;
        }

        private static ShipPlacement LongShip(SnapshotMessage snapshot)
        {
            for (int i = 0; i < snapshot.ships.Length; i++)
            {
                if (snapshot.ships[i].type == ShipType.Long)
                    return snapshot.ships[i];
            }

            throw new InvalidOperationException("No long ship.");
        }

        private static int WaterCell(SnapshotMessage board, bool[] used)
        {
            var occupied = new bool[board.width * board.height];
            for (int s = 0; s < board.ships.Length; s++)
            {
                int[] cells = board.ships[s].cells;
                for (int c = 0; c < cells.Length; c++)
                    occupied[cells[c]] = true;
            }

            for (int i = 0; i < occupied.Length; i++)
            {
                if (!occupied[i] && !used[i])
                    return i;
            }

            throw new InvalidOperationException("No water cell.");
        }

        private static ShotPlacement LastShot(Match match, int playerId)
        {
            ShotPlacement[] shots = match.SnapshotFor(playerId).shots;
            return shots[shots.Length - 1];
        }

        private static void PassTurn(Match match, int playerId, bool[] used)
        {
            SnapshotMessage board = match.SnapshotFor(1 - playerId);
            int cell = WaterCell(board, used);
            used[cell] = true;
            Assert.IsTrue(match.TryShoot(playerId, cell));
        }

        private static void PassTurn(Match match, int playerId)
        {
            PassTurn(match, playerId, new bool[36]);
        }
    }
}
