using System;

namespace Sudachi.Net.Core.Dictionary.Build
{
    public class Progress
    {
        private const long MS_100 = 100_000_000L; // 100ms in ticks
        private readonly int _maxUpdates;
        private readonly ICallback _callback;
        private float _currentProgress;
        private long _lastUpdate;

        public Progress(int maxUpdates, ICallback callback)
        {
            _maxUpdates = maxUpdates;
            _callback = callback;
        }

        public void StartBlock(string name, long start, Kind kind)
        {
            _lastUpdate = start;
            _callback.Start(name, kind);
            _currentProgress = Step();
        }

        private float Step() => 1.0f / _maxUpdates - 1e-6f;

        /**
         * This function limits calls to the progress function
         *
         * @param cur current state
         * @param max maximum state
         */

        public void ReportProgress(long cur, long max)
        {
            double ratio = cur / (double)max;
            if (ratio > _currentProgress)
            {
                if (ratio >= 1.0)
                {
                    _callback.Progress(1.0f);
                    _currentProgress = float.MaxValue;
                }

                long curTime = DateTime.Now.Ticks;
                if (curTime - _lastUpdate > MS_100)
                {
                    _callback.Progress((float)ratio);
                    float step = Step();
                    double nsteps = ratio / step;
                    _currentProgress += (float)(Math.Floor(nsteps) * step);
                    System.Diagnostics.Debug.Assert(ratio < _currentProgress);
                    _lastUpdate = curTime;
                }
            }
        }

        public void EndBlock(long size, TimeSpan time)
        {
            _callback.End(size, time);
        }

        public enum Kind
        {
            Input,
            Output
        }

        /**
         * Progress callback
         */

        public interface ICallback
        {
            /**
             * This function will be called for each step at the beginning
             *
             * @param name step name
             */

            void Start(string name, Kind kind);

            /**
             * This function will be called as progress is happening
             *
             * @param progress ratio of the progress
             */

            void Progress(float progress);

            void End(long size, TimeSpan time);
        }
    }
}
