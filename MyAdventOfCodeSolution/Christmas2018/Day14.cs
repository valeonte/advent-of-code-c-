using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Local

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day14 : Day
    {
        class Cookers
        {
            readonly List<int> _recipes = new List<int>(20_000_000);
            int _cooker1Idx, _cooker2Idx;

            public Cookers(IEnumerable<int> startRecipes)
            {
                _recipes.AddRange(startRecipes);
                _cooker1Idx = 0;
                _cooker2Idx = 1;
            }

            IEnumerable<int> Cook()
            {
                var recipe1 = _recipes[_cooker1Idx];
                var recipe2 = _recipes[_cooker2Idx];

                var newRecipeSum = recipe1 + recipe2;
                if (newRecipeSum > 9)
                {
                    var newRecipe2 = newRecipeSum / 10;
                    yield return newRecipe2;
                    _recipes.Add(newRecipe2);
                }
                var newRecipe1 = newRecipeSum % 10;
                yield return newRecipe1;
                _recipes.Add(newRecipe1);

                _cooker1Idx = MoveIdx(_cooker1Idx, 1 + recipe1);
                _cooker2Idx = MoveIdx(_cooker2Idx, 1 + recipe2);
            }

            int MoveIdx(int curIdx, int steps) => (curIdx + steps) % _recipes.Count;

            public IEnumerable<string> PrintRecipes()
            {
                for (var i = 0; i < _recipes.Count; i++)
                {
                    var r = _recipes[i];
                    if (_cooker1Idx == i) yield return $"({r})";
                    else if (_cooker2Idx == i) yield return $"[{r}]";
                    else yield return $" {r} ";
                }
            }

            void CookToTargetRecipes(int target)
            {
                while (_recipes.Count < target)
                {
                    foreach (var _ in Cook()) ;
                }
            }

            public IEnumerable<int> GetRecipesAfterCooked(int cooked, int afterCount)
            {
                CookToTargetRecipes(cooked);

                var returned = 0;
                while (_recipes.Count>cooked+returned && returned < afterCount)
                {
                    yield return _recipes[cooked + returned++];
                }

                while (returned < afterCount)
                {
                    foreach (var recipe in Cook())
                    {
                        yield return recipe;
                        if (returned++ == afterCount) break;
                    }
                }
            }

            IEnumerable<int> EnumerateAllForever()
            {
                foreach (var recipe in _recipes)
                {
                    yield return recipe;
                }
                while (true)
                {
                    // we have to fully enumerate Cook to get all cooked recipes in the array
                    var newRecipes = Cook().ToArray();
                    foreach (var recipe in newRecipes)
                    {
                        yield return recipe;
                    }
                }
            }

            public int FindFirstAppearanceOfSequence(IReadOnlyList<int> sequence)
            {
                var curIdx = 0;
                var cnt = 0;
                foreach (var recipe in EnumerateAllForever())
                {
                    cnt++;
                    if (curIdx > 0 && recipe != sequence[curIdx]) curIdx = 0;

                    if (recipe == sequence[curIdx]) curIdx++;

                    if (curIdx == sequence.Count) return cnt - sequence.Count;
                }

                throw new InvalidOperationException();
            }
        }

        public override dynamic Answer1()
        {
            var cookers = new Cookers(new[] {3, 7});

            //while (true)
            //{
            //    Console.WriteLine(string.Join(string.Empty, cookers.PrintRecipes()));
            //    Console.ReadKey();
            //    var newRecipes = cookers.Cook().ToArray();
            //}

            //foreach (var testCooked in new int[] {9,5,18,2018})
            //{
            //    var res = cookers.GetRecipesAfterCooked(testCooked, 10);
            //    Console.WriteLine(string.Join("", res));
            //}

            var res = cookers.GetRecipesAfterCooked(919901, 10);

            return string.Join("", res);
        }

        public override dynamic Answer2()
        {
            var cookers = new Cookers(new[] {3, 7});

            //foreach (var testSequence in new [] {"51589", "01245", "92510", "59414"})
            //{
            //    var res = cookers.FindFirstAppearanceOfSequence(testSequence.Select(c => (int) char.GetNumericValue(c)).ToArray());
            //    Console.WriteLine(res);
            //}

            return cookers.FindFirstAppearanceOfSequence("919901".Select(c => (int)char.GetNumericValue(c)).ToArray());
        }
    }
}