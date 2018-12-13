using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Local

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day13 : Day
    {
        // ReSharper disable once UnusedMember.Local
        const string TestInput = @"/->-\        
|   |  /----\
| /-+--+-\  |
| | |  | v  |
\-+-/  \-+--/
  \------/   ";

        const string TestInput2 = @"/>-<\  
|   |  
| /<+-\
| | | v
\>+</ |
  |   ^
  \<->/";

        /// <summary>
        /// To turn right increase, to turn left decrease and mod 4
        /// </summary>
        enum Direction
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        }

        class Cart
        {
            public int X { get; private set; }
            public int Y { get; private set; }
            public Direction Direction { get; private set; }

            public Cart(int x, int y, Direction direction)
            {
                X = x;
                Y = y;
                Direction = direction;
            }

            public void Move()
            {
                switch (Direction)
                {
                    case Direction.Down:
                        X += 1;
                        break;
                    case Direction.Up:
                        X -= 1;
                        break;
                    case Direction.Left:
                        Y -= 1;
                        break;
                    case Direction.Right:
                        Y += 1;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            /// <summary>
            /// Turn Right 4 times is going straight, turn right 3 times is turning left
            /// </summary>
            /// <param name="times"></param>
            void TurnRight(int times)
            {
                Direction = (Direction) (((int) Direction + times) % 4);
            }

            int _intersectionCounter;
            public void AdjustDirection(RoadElement roadElement)
            {
                var oldDirection = Direction;

                switch (roadElement)
                {
                    case RoadElement.Horizontal:
                    case RoadElement.Vertical:
                        // no change in direction
                        return;

                    case RoadElement.TopLeftBottomRight:
                        // this is essentialy 1 (turn right) for up or down and 3 (turn left) for left or right
                        TurnRight((int) oldDirection % 2 * 2 + 1);
                        return;
                    case RoadElement.TopRightBottomLeft:
                        // this is essentialy 3 (turn left) for up or down and 1 (turn right) for left or right
                        TurnRight((int) (oldDirection + 1) % 2 * 2 + 1);
                        return;
                    case RoadElement.Intersection:
                        // increase the intersection counter, and do the right thing
                        switch (_intersectionCounter++ % 3)
                        {
                            case 0:
                                // left turn
                                TurnRight(3);
                                return;
                            case 1:
                                // straight
                                return;
                            case 2:
                                // right turn
                                TurnRight(1);
                                return;
                            default:
                                throw new InvalidOperationException();
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override string ToString()
            {
                return $"{Y}, {X}, {Direction}";
            }
        }

        enum RoadElement
        {
            Horizontal,
            Vertical,
            TopLeftBottomRight,
            TopRightBottomLeft,
            Intersection
        }

        class RoadNetwork
        {
            static readonly IDictionary<char, RoadElement?> CharToElementMap = new Dictionary<char, RoadElement?>
            {
                {' ', null},
                {'-', RoadElement.Horizontal},
                {'|', RoadElement.Vertical},
                {'/', RoadElement.TopLeftBottomRight},
                {'\\', RoadElement.TopRightBottomLeft},
                {'+', RoadElement.Intersection},

                {'>', RoadElement.Horizontal},
                {'<', RoadElement.Horizontal},

                {'^', RoadElement.Vertical},
                {'v', RoadElement.Vertical}
            };

            static readonly IDictionary<char, Direction> CharToCartMap = new Dictionary<char, Direction>
            {
                {'>', Direction.Right},
                {'<', Direction.Left},

                {'^', Direction.Up},
                {'v', Direction.Down}
            };

            readonly RoadElement?[,] _elements;
            readonly IList<Cart> _carts = new List<Cart>();

            public IReadOnlyCollection<Cart> Carts => (IReadOnlyCollection<Cart>) _carts;

            public RoadNetwork(IReadOnlyList<string> stringNetwork)
            {
                _elements = new RoadElement?[stringNetwork.Count, stringNetwork[0].Length];
                PopulateRoadNetwork(stringNetwork);
            }

            
            void PopulateRoadNetwork(IReadOnlyList<string> stringNetwork)
            {
                for (var line = 0; line < stringNetwork.Count; line++)
                {
                    var networkLine = stringNetwork[line];
                    for (var col = 0; col < networkLine.Length; col++)
                    {
                        var networkChar = networkLine[col];
                        _elements[line, col] = CharToElementMap[networkChar];

                        if (!CharToCartMap.TryGetValue(networkChar, out var direction)) continue;
                        _carts.Add(new Cart(line, col, direction));
                    }
                }
            }

            public void MoveTick(out List<Cart> collidedCarts)
            {
                collidedCarts = new List<Cart>();
                foreach (var cart in _carts.OrderBy(c=>c.X).ThenBy(c=>c.Y))
                {
                    if (collidedCarts.Contains(cart)) continue;

                    MoveCart(cart, out var moveCollidedCarts);
                    if (moveCollidedCarts.Length < 2) continue;

                    collidedCarts.AddRange(moveCollidedCarts);
                }

                foreach (var collidedCart in collidedCarts)
                {
                    _carts.Remove(collidedCart);
                }
            }

            void MoveCart(Cart cart, out Cart[] collidedCarts)
            {
                cart.Move();
                collidedCarts = _carts.Where(c => c.X == cart.X && c.Y == cart.Y).ToArray();
                if (collidedCarts.Length > 1)
                {
                    return;
                }

                var newElement = _elements[cart.X, cart.Y];
                if (!newElement.HasValue) throw new InvalidOperationException();

                cart.AdjustDirection(newElement.Value);
            }

            public void PrintNetwork()
            {
                var rows = _elements.GetLength(0);
                var cols = _elements.GetLength(1);
                for (var row = 0; row < rows; row++)
                {
                    for (var col = 0; col < cols; col++)
                    {
                        var carts = _carts.Where(c => c.X == row && c.Y == col).ToArray();
                        char ch;
                        switch (carts.Length)
                        {
                            case 0:
                                ch = CharToElementMap.First(p => p.Value == _elements[row, col]).Key;
                                break;
                            case 1:
                                ch = CharToCartMap.First(p => p.Value == carts[0].Direction).Key;
                                break;
                            case 2:
                                ch = 'X';
                                break;
                            default:
                                throw new InvalidOperationException();
                        }

                        Console.Write(ch);
                    }

                    Console.WriteLine();
                }

                Console.WriteLine($"{_carts.Count} total carts left");
            }
        }

        public override dynamic Answer1()
        {
            //var input = TestInput.Split(Environment.NewLine);
            var input = GetRawInput();

            var network = new RoadNetwork(input);

            //network.PrintNetwork();

            List<Cart> collidedCarts;
            do
            {
                //Console.ReadKey();
                network.MoveTick(out collidedCarts);

                //network.PrintNetwork();
            } while (collidedCarts.Count == 0);

            return $"{collidedCarts[0]}";
        }

        public override dynamic Answer2()
        {
            //var input = TestInput.Split(Environment.NewLine);
            //var input = TestInput2.Split(Environment.NewLine);
            var input = GetRawInput();

            var network = new RoadNetwork(input);

            //network.PrintNetwork();

            do
            {
                //Console.ReadKey();
                network.MoveTick(out var _);

                //network.PrintNetwork();
            } while (network.Carts.Count > 1);

            return $"{network.Carts.First()}";
        }
    }
}