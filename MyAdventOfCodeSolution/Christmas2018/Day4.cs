using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyAdventOfCodeSolution.Christmas2018
{
    class Day4 : Day
    {
        enum GuardAction
        {
            BeginShift, FallAsleep, WakeUp
        }

        class GuardRecord
        {
            static readonly IDictionary<string, GuardAction> GuardActionMap = new Dictionary<string, GuardAction>
            {
                {"begins shift", GuardAction.BeginShift},
                {"falls asleep", GuardAction.FallAsleep},
                {"wakes up", GuardAction.WakeUp}
            };

            static readonly Regex GuardRecordRegex = new Regex($@"\[(.+)\]( Guard #(\d+))? ({string.Join('|', GuardActionMap.Keys)})");

            public DateTime Timestamp { get; }

            public int? GuardId { get; }
            public GuardAction GuardAction { get; }

            public GuardRecord(string guardRecordString)
            {
                var match = GuardRecordRegex.Match(guardRecordString);
                if (!match.Success)
                    throw new InvalidOperationException("Invalid guard record string!");

                Timestamp = DateTime.Parse(match.Groups[1].Value);
                GuardAction = GuardActionMap[match.Groups[4].Value];
                if (GuardAction == GuardAction.BeginShift)
                    GuardId = int.Parse(match.Groups[3].Value);
            }
        }

        IDictionary<int, int[]> _guardsMinutesAsleep;
        IDictionary<int, int[]> GuardsMinutesAsleep => _guardsMinutesAsleep ?? (_guardsMinutesAsleep = GetGuardsMinutesAsleep());

        IDictionary<int, int[]> GetGuardsMinutesAsleep()
        {
            var guardsMinutesAsleep = new Dictionary<int, int[]>();

            int? curGuardId = null;
            int? asleepMinute = null;

            foreach (var guardRecord in GetRawInput().Select(s => new GuardRecord(s)).OrderBy(g => g.Timestamp))
            {
                switch (guardRecord.GuardAction)
                {
                    case GuardAction.BeginShift:
                        curGuardId = guardRecord.GuardId;
                        continue;

                    case GuardAction.FallAsleep:
                        if (!curGuardId.HasValue) throw new InvalidOperationException("Cur Guard Id has no value at asleep, impossible!!");
                        asleepMinute = guardRecord.Timestamp.Minute;
                        continue;

                    case GuardAction.WakeUp:
                        if (!curGuardId.HasValue) throw new InvalidOperationException("Cur Guard Id has no value at wake up, impossible!!");
                        if (!asleepMinute.HasValue) throw new InvalidOperationException("Asleep Minute has no value at wake up, impossible!!");

                        if (!guardsMinutesAsleep.TryGetValue(curGuardId.Value, out var minutesAsleep))
                        {
                            minutesAsleep = new int[60];
                            guardsMinutesAsleep.Add(curGuardId.Value, minutesAsleep);
                        }

                        var awakeMinute = guardRecord.Timestamp.Minute;
                        for (var i = asleepMinute.Value; i < awakeMinute; i++)
                        {
                            minutesAsleep[i]++;
                        }

                        asleepMinute = null;

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return guardsMinutesAsleep;
        }

        static (int MostlySleptMinute, int TimesSleptOnMostlySleptMinute) GetMostlySleptMinute(IReadOnlyList<int> minutesSlept)
        {
            if (minutesSlept.Count != 60)
                throw new InvalidOperationException("This will only work for 60 minutes");

            var mostlySleptMinute = -1;
            var timesSleptOnMostlySleptMinute = -1;
            for (var i = 0; i < 60; i++)
            {
                if (minutesSlept[i] <= timesSleptOnMostlySleptMinute) continue;
                mostlySleptMinute = i;
                timesSleptOnMostlySleptMinute = minutesSlept[i];
            }

            return (mostlySleptMinute, timesSleptOnMostlySleptMinute);
        }

        public override dynamic Answer1()
        {
            var mostAsleepGuard = GuardsMinutesAsleep.OrderByDescending(g => g.Value.Sum()).First();

            var mostlySleptMinute = GetMostlySleptMinute(mostAsleepGuard.Value).MostlySleptMinute;

            return mostAsleepGuard.Key * mostlySleptMinute;
        }

        public override dynamic Answer2()
        {
            var curSleeper = -1;
            var curMostlySleptMinute = -1;
            var curMaxTimesSleptOnMostlySleptMinute = -1;
            foreach (var guard in GuardsMinutesAsleep)
            {
                var mostlySlept = GetMostlySleptMinute(guard.Value);
                if (mostlySlept.TimesSleptOnMostlySleptMinute <= curMaxTimesSleptOnMostlySleptMinute) continue;

                curSleeper = guard.Key;
                curMostlySleptMinute = mostlySlept.MostlySleptMinute;
                curMaxTimesSleptOnMostlySleptMinute = mostlySlept.TimesSleptOnMostlySleptMinute;
            }

            return curSleeper * curMostlySleptMinute;
        }
    }
}