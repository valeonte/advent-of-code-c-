using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// ReSharper disable UnusedMember.Local

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day20 : Day
    {
        struct Coords : IEquatable<Coords>, IComparable<Coords>, IComparable
        {
            public Coords(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }
            public int Y { get; }

            public int CompareTo(Coords other)
            {
                if (Y < other.Y) return -1;
                if (Y > other.Y) return 1;

                if (X < other.X) return -1;
                if (X > other.X) return 1;

                return 0;
            }

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                return obj is Coords coords && Equals(coords);
            }

            public bool Equals(Coords other)
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

            public override string ToString() => $"({X}, {Y})";
            public int CompareTo(object obj)
            {
                if (obj is Coords other) return CompareTo(other);
                return 1;
            }
        }

        class Room : IEquatable<Room>
        {
            public Room(Facility facility, Coords coords, int distance)
            {
                Facility = facility;
                Coords = coords;
                Distance = distance;
            }

            public Coords Coords { get; }
            public Facility Facility { get; }
            public int Distance { get; }

            public Room North { get; private set; }
            public Room South { get; private set; }
            public Room East { get; private set; }
            public Room West { get; private set; }

            public Room AddNorth()
            {
                if (North == null)
                {
                    var room = new Room(Facility, new Coords(Coords.X, Coords.Y - 1), Distance + 1);
                    Facility.AddRoom(room);
                    North = room;
                }

                North.AddSouth(this);
                return North;
            }

            void AddNorth(Room room)
            {
                if (North == null)
                {
                    North = room;
                    return;
                }

                if (!North.Equals(room)) throw new InvalidOperationException();
            }

            public Room AddSouth()
            {
                if (South == null)
                {
                    var room = new Room(Facility, new Coords(Coords.X, Coords.Y + 1), Distance + 1);
                    Facility.AddRoom(room);
                    South = room;
                }

                South.AddNorth(this);
                return South;
            }

            void AddSouth(Room room)
            {
                if (South == null)
                {
                    South = room;
                    return;
                }

                if (!South.Equals(room)) throw new InvalidOperationException();
            }

            public Room AddEast()
            {
                if (East == null)
                {
                    var room = new Room(Facility, new Coords(Coords.X + 1, Coords.Y), Distance + 1);
                    Facility.AddRoom(room);
                    East = room;
                }

                East.AddWest(this);
                return East;
            }

            void AddEast(Room room)
            {
                if (East == null)
                {
                    East = room;
                    return;
                }

                if (!East.Equals(room)) throw new InvalidOperationException();
            }

            public Room AddWest()
            {
                if (West == null)
                {
                    var room = new Room(Facility, new Coords(Coords.X - 1, Coords.Y), Distance + 1);
                    Facility.AddRoom(room);
                    West = room;
                }

                West.AddEast(this);
                return West;
            }

            void AddWest(Room room)
            {
                if (West == null)
                {
                    West = room;
                    return;
                }

                if (!West.Equals(room)) throw new InvalidOperationException();
            }

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Room) obj);
            }

            public bool Equals(Room other)
            {
                if (other is null) return false;
                return ReferenceEquals(this, other) || Coords.Equals(other.Coords);
            }

            public override int GetHashCode()
            {
                return Coords.GetHashCode();
            }

            public override string ToString() => $"Room {Coords}";
        }

        class Facility
        {
            int _minX, _minY, _maxX, _maxY;

            public Facility()
            {
                Start = new Room(this, new Coords(0, 0), 0);
                Rooms.Add(Start);
            }

            public Room Start { get; }
            public IList<Room> Rooms { get; } = new List<Room>();

            public void AddRoom(Room room)
            {
                if (room.Coords.X < _minX) _minX = room.Coords.X;
                if (room.Coords.Y < _minY) _minY = room.Coords.Y;

                if (room.Coords.X > _maxX) _maxX = room.Coords.X;
                if (room.Coords.Y > _maxY) _maxY = room.Coords.Y;

                Rooms.Add(room);
            }

            public void Print()
            {
                var dimX = 2 * (_maxX - _minX + 1) + 1;
                var dimY = 2 * (_maxY - _minY + 1) + 1;
                var arr = new char?[dimX, dimY];

                foreach (var room in Rooms)
                {
                    var roomX = 2 * (room.Coords.X - _minX) + 1;
                    var roomY = 2 * (room.Coords.Y - _minY) + 1;
                    arr[roomX, roomY] = room.Coords.X == 0 && room.Coords.Y == 0 ? 'X' : '.';

                    if (room.North != null) arr[roomX, roomY - 1] = '-';
                    else arr[roomX, roomY - 1] = '#';

                    if (room.South != null) arr[roomX, roomY + 1] = '-';
                    else arr[roomX, roomY + 1] = '#';

                    if (room.West != null) arr[roomX - 1, roomY] = '|';
                    else arr[roomX - 1, roomY] = '#';

                    if (room.East != null) arr[roomX + 1, roomY] = '|';
                    else arr[roomX + 1, roomY] = '#';
                }

                Console.WriteLine(new string('-', 100));
                for (var y = 0; y < dimY; y++)
                {
                    var line = new char[dimX];
                    for (var x = 0; x < dimX; x++)
                    {
                        line[x] = arr[x, y] ?? '#';
                    }
                    Console.WriteLine(new string(line));
                }
            }

            public override string ToString() => $"{Rooms.Count} rooms facility";
        }

        Facility _facility;
        public override dynamic Answer1()
        {
            //var input = "^WNE$";
            //var input = "^ENWWW(NEEE|SSE(EE|N))$";
            //var input = "^ENNWSWW(NEWS|)SSSEEN(WNSE|)EE(SWEN|)NNN$";
            //var input = "^ESSWWN(E|NNENN(EESS(WNSE|)SSS|WWWSSSSE(SW|NNNE)))$";
            //var input = "^WSSEESWWWNW(S|NENNEEEENN(ESSSSW(NWSW|SSEN)|WSWWN(E|WWS(E|SS))))$";
            var input = GetRawInput()[0];

            _facility = new Facility();
            var cur = _facility.Start;
            var branches = new Stack<Room>();
            foreach (var d in input)
            {
                if (d == '^' || d == '$') continue;

                switch (d)
                {
                    case 'N':
                        cur = cur.AddNorth();
                        break;
                    case 'S':
                        cur = cur.AddSouth();
                        break;
                    case 'E':
                        cur = cur.AddEast();
                        break;
                    case 'W':
                        cur = cur.AddWest();
                        break;
                    case '(':
                        branches.Push(cur);
                        break;
                    case '|':
                        cur = branches.Peek();
                        break;
                    case ')':
                        cur = branches.Pop();
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                //fac.Print();

                //Console.ReadKey();
            }

            //fac.Print();
            //Console.ReadKey();
            var maxDistance = _facility.Rooms.Select(r => r.Distance).Max();

            return maxDistance;
        }

        public override dynamic Answer2()
        {
            var result = _facility.Rooms.Count(r => r.Distance > 999);
            return result;
        }
    }
}