using System.Collections.Generic;
using System.Linq;

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day9 : Day
    {
        class MarbleCircle
        {
            readonly LinkedList<int> _marbles;

            public MarbleCircle()
            {
                _marbles = new LinkedList<int>(new[] {0});
                _currentNode = _marbles.First;
            }

            LinkedListNode<int> _currentNode;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="signedDistance">Positive clockwise, negative counterclockwise</param>
            /// <returns></returns>
            LinkedListNode<int> GetMarbleAt(int signedDistance)
            {
                return signedDistance > 0 ? ClockwiseFromCurrent(signedDistance) : CounterclockwiseFromCurrent(-signedDistance);
            }

            LinkedListNode<int> ClockwiseFromCurrent(int distance)
            {
                var ret = _currentNode;
                for (var i = 0; i < distance; i++)
                {
                    ret = ret.Next ?? _marbles.First;
                }

                return ret;
            }

            LinkedListNode<int> CounterclockwiseFromCurrent(int distance)
            {
                var ret = _currentNode;

                for (var i = 0; i < distance; i++)
                {
                    ret = ret.Previous ?? _marbles.Last;
                }

                return ret;
            }

            public void AddMarbleAfter(int distance, int marble)
            {
                var prevMarble = GetMarbleAt(distance);
                _marbles.AddAfter(prevMarble, marble);
            }

            public int RemoveMarble(int distance)
            {
                var toBeRemoved = GetMarbleAt(distance);
                _marbles.Remove(toBeRemoved);
                return toBeRemoved.Value;
            }

            public void SetCurrentAt(int distance)
            {
                _currentNode = GetMarbleAt(distance);
            }
        }

        class MarbleGame
        {
            readonly MarbleCircle _circle;

            readonly IDictionary<int, long> _playerScores = new Dictionary<int, long>();
            readonly int _players;

            public MarbleGame(int players)
            {
                _circle = new MarbleCircle();
                for (var i = 0; i < players; i++)
                {
                    _playerScores.Add(i + 1, 0);
                }

                _players = players;
                _lastPlayer = players;
            }

            int _nextMarble = 1;
            int _lastPlayer;

            int GetNextPlayer()
            {
                return _lastPlayer == _players ? 1 : _lastPlayer + 1;
            }

            public IDictionary<int, long> Play(int moves)
            {
                for (var i = 0; i < moves; i++)
                {
                    var player = GetNextPlayer();
                    var nextMarble = _nextMarble++;
                    if (nextMarble % 23 == 0)
                    {
                        _playerScores[player] += nextMarble;
                        // removing
                        var removedMarble = _circle.RemoveMarble(-7);
                        // adding the score
                        _playerScores[player] += removedMarble;

                        // setting current to the next of the one to be deleted
                        _circle.SetCurrentAt(-6);
                    }
                    else
                    {
                        _circle.AddMarbleAfter(1, nextMarble);
                        // very special case, in a circle or 2 nodes, the node next to the next one, is not distance 2 away :/
                        _circle.SetCurrentAt(nextMarble == 1 ? 1 : 2);
                    }

                    _lastPlayer = player;
                }

                return _playerScores;
            }
        }

        public override dynamic Answer1()
        {
            var game = new MarbleGame(486);
            var results = game.Play(70833);

            var winner = results.OrderByDescending(r => r.Value).First();
            return $"Player {winner.Key} won with a score of {winner.Value}";
        }

        public override dynamic Answer2()
        {
            var game = new MarbleGame(486);
            var results = game.Play(7083300);

            var winner = results.OrderByDescending(r => r.Value).First();
            return $"Player {winner.Key} won with a score of {winner.Value}";
        }
    }
}