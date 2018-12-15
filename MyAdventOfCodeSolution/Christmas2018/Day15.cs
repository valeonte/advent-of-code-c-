using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedMember.Local

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day15 : Day
    {
        class PathPosition : IComparable, IComparable<PathPosition>, IEquatable<PathPosition>
        {
            public PathPosition(Position position, PathPosition parentPosition = null)
            {
                Position = position;
                ParentPosition = parentPosition;
                PathLength = parentPosition?.PathLength + 1 ?? 0;
            }

            public Position Position { get; }
            public PathPosition ParentPosition { get; }
            public int PathLength { get; }


            public int CompareTo(object obj)
            {
                if (obj is null || !(obj is PathPosition other)) return 1;
                return CompareTo(other);
            }

            public int CompareTo(PathPosition other)
            {
                return other is null ? 1 : Position.CompareTo(other.Position);
            }

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((PathPosition) obj);
            }

            public bool Equals(PathPosition other)
            {
                if (other is null) return false;
                return ReferenceEquals(this, other) || Position.Equals(other.Position);
            }

            public override int GetHashCode()
            {
                return Position.GetHashCode();
            }

            public override string ToString()
            {
                return Position.ToString();
            }
        }

        struct Position : IEquatable<Position>, IComparable<Position>
        {
            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }

            public override string ToString()
            {
                return $"({X}, {Y})";
            }

            public int CompareTo(Position other)
            {
                if (Y < other.Y) return -1;
                if (Y > other.Y) return 1;

                // same Y
                if (X < other.X) return -1;
                if (X > other.X) return 1;

                // same X and Y
                return 0;
            }

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                return obj is Position position && Equals(position);
            }

            public bool Equals(Position other)
            {
                return X == other.X && Y == other.Y;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (X * 397) ^ Y;
                }
            }
        }

        class Game
        {
            readonly int _dimX;
            readonly int _dimY;

            public abstract class Element
            {
                protected Element(int x, int y)
                {
                    Position = new Position(x, y);
                }

                public Position Position { get; protected set; }
            }

            public abstract class Fighter : Element
            {
                public abstract Type EnemyType { get; }

                protected Fighter(int x, int y) : base(x, y)
                {
                }

                public int Hitpoints { get; set; }
                public int Damage { private get; set; }
                public bool Dead => Hitpoints <= 0;


                public void MoveTo(Position pos)
                {
                    if (DebugMode) Console.WriteLine($"{this} moves to {pos}");
                    Position = pos;
                }

                public void Attack(Fighter enemy)
                {
                    if (DebugMode) Console.WriteLine($"{this} attacks {enemy}");
                    enemy.Hitpoints -= Damage;
                }
            }

            sealed class Wall : Element
            {
                public Wall(int x, int y) : base(x, y)
                {
                }
            }

            sealed class Elf : Fighter
            {
                public override Type EnemyType => typeof(Goblin);

                public Elf(int x, int y) : base(x, y)
                {
                }

                public override string ToString()
                {
                    return $"Elf{Position}[{Hitpoints}]";
                }
            }

            sealed class Goblin : Fighter
            {
                public override Type EnemyType => typeof(Elf);

                public Goblin(int x, int y) : base(x, y)
                {
                }

                public override string ToString()
                {
                    return $"Goblin{Position}[{Hitpoints}]";
                }
            }

            // this keeps track of occupied tiles
            readonly Element[,] _map;

            public IReadOnlyList<Fighter> Elves => _elves;
            public IReadOnlyList<Fighter> Goblins => _goblins;

            readonly List<Fighter> _elves = new List<Fighter>();
            readonly List<Fighter> _goblins = new List<Fighter>();

            Game(int dimX, int dimY)
            {
                _dimX = dimX;
                _dimY = dimY;
                _map = new Element[dimX, dimY];
            }

            void AddElement(int x, int y, char element, int goblinHp = 200, int elfHp = 200, int goblinAp = 3, int elfAp = 3)
            {
                switch (element)
                {
                    case 'E':
                        var elf = new Elf(x, y) {Damage = elfAp, Hitpoints = elfHp};
                        _elves.Add(elf);
                        _map[x, y] = elf;
                        break;
                    case 'G':
                        var goblin = new Goblin(x, y) {Damage = goblinAp, Hitpoints = goblinHp};
                        _goblins.Add(goblin);
                        _map[x, y] = goblin;
                        break;
                    case '#':
                        _map[x, y] = new Wall(x, y);
                        break;
                    case '.':
                        _map[x, y] = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(element));
                }
            }

            public IEnumerable<PathPosition> EnumerateAllPaths(Position source, ICollection<Position> targets)
            {
                var allSources = new List<PathPosition> {new PathPosition(source)};
                var visited = new HashSet<Position> {source};

                do
                {
                    var adjacent = GetMultipleAdjacentInOrder(allSources).Where(e => !visited.Contains(e.Position)).ToArray();
                    var newSources = new List<PathPosition>();

                    foreach (var pathPosition in adjacent)
                    {
                        if (targets.Contains(pathPosition.Position))
                        {
                            // success
                            yield return pathPosition;
                            yield break;
                        }

                        // collision detection
                        if (_map[pathPosition.Position.X, pathPosition.Position.Y] != null) continue;

                        newSources.Add(pathPosition);
                        visited.Add(pathPosition.Position);
                        yield return pathPosition;
                    }

                    allSources = newSources;
                } while (allSources.Count > 0);

                // could not get to target
                yield return null;
            }

            IEnumerable<PathPosition> GetMultipleAdjacentInOrder(IEnumerable<PathPosition> positions)
            {
                return positions.SelectMany(GetAdjacentInOrder).Distinct().OrderBy(p => p);
            }

            IEnumerable<PathPosition> GetAdjacentInOrder(PathPosition position)
            {
                for (var y = -1; y <= 1; y++)
                for (var x = -1; x <= 1; x++)
                {
                    if (x == y || x != 0 && y != 0) continue;
                    var newX = position.Position.X + x;
                    var newY = position.Position.Y + y;

                    if (newX < 0 || newY < 0 || newX >= _dimX || newY >= _dimY) continue;

                    yield return new PathPosition(new Position(newX, newY), position);
                }
            }

            public void PrintMap(ICollection<Position> highlight = null)
            {
                for (var y = 0; y < _dimY; y++)
                {
                    var line = new char[_dimX];
                    var fightersInLine = new List<string>();
                    for (var x = 0; x < _dimX; x++)
                    {
                        var element = _map[x, y];

                        char ch;
                        if (highlight != null && highlight.Contains(new Position(x, y)))
                        {
                            ch = 'X';
                        }
                        else if (element is null)
                        {
                            ch = '.';
                        }
                        else if (element is Wall)
                        {
                            ch = '#';
                        }
                        else if (element is Elf elf)
                        {
                            ch = 'E';
                            fightersInLine.Add($"E({elf.Hitpoints})");
                        }
                        else if (element is Goblin goblin)
                        {
                            ch = 'G';
                            fightersInLine.Add($"G({goblin.Hitpoints})");
                        }
                        else throw new InvalidOperationException();

                        line[x] = ch;
                    }

                    Console.WriteLine($"{new string(line)} {string.Join(", ", fightersInLine)}");
                }
            }

            public bool Move()
            {
                var played = new HashSet<Fighter>();
                for (var y = 0; y < _dimY; y++)
                for (var x = 0; x < _dimY; x++)
                {
                    var element = _map[x, y];
                    if (element == null || !(element is Fighter fighter) || played.Contains(fighter)) continue;

                    if (Goblins.Count == 0 || Elves.Count == 0) return false; // no enemies, round incomplete!

                    played.Add(fighter);
                    var nextMove = GetFighterNextPosition(element, out var nearestTarget);
                    if (!nextMove.HasValue) continue;

                    var newX = nextMove.Value.X;
                    var newY = nextMove.Value.Y;
                    var nextSpot = _map[newX, newY];
                    if (nextSpot == null)
                    {
                        fighter.MoveTo(nextMove.Value);
                        _map[x, y] = null;
                        _map[newX, newY] = fighter;
                    }

                    // path length 1 is next, 2 is 1 step away
                    // in both case we should attack!
                    if (nearestTarget.PathLength > 2) continue;

                    var enemy = GetAdjacentInOrder(new PathPosition(fighter.Position))
                        .Select(a => _map[a.Position.X, a.Position.Y])
                        .Where(f => !(f is null) && f.GetType() == fighter.EnemyType)
                        .Cast<Fighter>()
                        .OrderBy(f => f.Hitpoints)
                        .ThenBy(f => f.Position)
                        .FirstOrDefault();

                    if (enemy == null) throw new InvalidOperationException();

                    fighter.Attack(enemy);
                    if (!enemy.Dead) continue;

                    _elves.Remove(enemy);
                    _goblins.Remove(enemy);
                    _map[enemy.Position.X, enemy.Position.Y] = null;

                    //PrintMap();
                    //Console.ReadKey();
                }

                return true;
            }

            Position? GetFighterNextPosition(Element fighter, out PathPosition nearestTarget)
            {
                var targets = new HashSet<Position>(fighter is Elf ? _goblins.Select(g => g.Position) : _elves.Select(g => g.Position));
                nearestTarget = EnumerateAllPaths(fighter.Position, targets).LastOrDefault();
                if (nearestTarget == null) return null; // fighter cannot move

                var lastNotNullParent = nearestTarget;

                while (lastNotNullParent.ParentPosition != null && !lastNotNullParent.ParentPosition.Position.Equals(fighter.Position))
                {
                    lastNotNullParent = lastNotNullParent.ParentPosition;
                }

                return lastNotNullParent.Position;
            }

            public static Game FromStringInput(IReadOnlyList<string> input, int goblinHp = 200, int elfHp = 200, int goblinAp = 3, int elfAp = 3)
            {
                var dimX = input[0].Length;
                var dimY = input.Count;
                var game = new Game(dimX, dimY);

                for (var y = 0; y < dimY; y++)
                {
                    var line = input[y];
                    for (var x = 0; x < line.Length; x++)
                    {
                        game.AddElement(x, y, line[x], goblinHp, elfHp, goblinAp, elfAp);
                    }
                }

                return game;
            }
        }

        const string TestInput = @"#########
#G..G..G#
#.......#
#.......#
#G..E..G#
#.......#
#.......#
#G..G..G#
#########";

        const string TestInput2 = @"#########
#G......#
#.E.#...#
#..##..G#
#...##..#
#...#...#
#.G...G.#
#.....G.#
#########";

        public static bool DebugMode = false;

        public override dynamic Answer1()
        {
            //var input = TestInput2.Split(Environment.NewLine);
            var game = Game.FromStringInput(GetRawInput()); 

            //TestPathCreation(game);

            var round = 0;
            do
            {
                if (DebugMode) game.PrintMap();
                //Console.ReadKey();
                round++;
                if (DebugMode) Console.WriteLine($"-------------------- Round {round} ----------------------------");
            } while (game.Move());

            var sumOfRemaining = game.Elves.Sum(e => e.Hitpoints) + game.Goblins.Sum(g => g.Hitpoints);
            var result = sumOfRemaining * (round - 1);

            return result;
        }

        public override dynamic Answer2()
        {
            //var input = TestInput2.Split(Environment.NewLine);
            var input = GetRawInput();

            var elfAp = 4;
            Game game;
            int round;
            do
            {
                game = Game.FromStringInput(input, elfAp: elfAp);
                var originalElfCount = game.Elves.Count;

                round = 0;
                do
                {
                    round++;
                    if (!game.Move()) break;
                } while (game.Elves.Count == originalElfCount);

                if (game.Elves.Count == originalElfCount) break;

                if (DebugMode) Console.WriteLine($"Elf AP: {elfAp}, elf died after {round} rounds");

                elfAp++;

            } while (true);

            var sumOfRemaining = game.Elves.Sum(e => e.Hitpoints) + game.Goblins.Sum(g => g.Hitpoints);
            var result = sumOfRemaining * (round - 1);

            return result;
        }

        static void TestPathCreation(Game game)
        {
            var elf = game.Elves[0];

            var source = elf.Position;
            var targets = new HashSet<Position>(game.Goblins.Select(g => g.Position));

            var visited = new HashSet<Position>();

            PathPosition lastPathPosition = null;
            foreach (var pathPosition in game.EnumerateAllPaths(source, targets))
            {
                lastPathPosition = pathPosition;
                visited.Add(pathPosition.Position);
                //game.PrintMap(visited);
                //Console.ReadKey();
            }

            if (lastPathPosition == null) throw new InvalidOperationException();

            var path = new HashSet<Position>();
            while (lastPathPosition.ParentPosition != null && !lastPathPosition.ParentPosition.Position.Equals(elf.Position))
            {
                lastPathPosition = lastPathPosition.ParentPosition;
                path.Add(lastPathPosition.Position);
            }

            game.PrintMap(path);
        }
    }
}