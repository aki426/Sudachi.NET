using System;

namespace Sudachi.Net.Core.Dictionary.Build
{
    /// <summary>
    /// ICallBackをラップして進捗状況を管理し100ms以上の間隔で処理を実行するクラス。
    /// </summary>
    public class Progress
    {
        private static readonly long MS_100 = 1_000_000L; // 100ms in ticks (one tick is 100ns in C#)

        private readonly int _maxUpdates;
        private readonly ICallback _callback;
        private float _currentProgress;
        private long _lastUpdate; // C# ではDateTime.Now.Ticksを使うため原単位が100nsになる点に注意。

        /// <summary>
        /// Initializes a new instance of the <see cref="Progress"/> class.
        /// </summary>
        /// <param name="maxUpdates">The max updates.</param>
        /// <param name="callback">The callback.</param>
        public Progress(int maxUpdates, ICallback callback)
        {
            _maxUpdates = maxUpdates;
            _callback = callback;
        }

        /// <summary>
        /// Starts the block.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="start">The start.</param>
        /// <param name="kind">The kind.</param>
        public void StartBlock(string name, long start, Kind kind)
        {
            _lastUpdate = start;
            _callback.Start(name, kind);
            _currentProgress = Step();
        }

        /// <summary>
        /// one progress step value.
        /// </summary>
        /// <returns>A float.</returns>
        private float Step() => 1.0f / _maxUpdates - 1e-6f;

        /// <summary>
        /// 100ms以上の間隔でICallbackを実行するように制限する関数。
        /// </summary>
        /// <param name="cur">current state</param>
        /// <param name="max">maximum state</param>
        public void LimitedProgress(long cur, long max)
        {
            double ratio = cur / (double)max;
            if (ratio > _currentProgress)
            {
                if (ratio >= 1.0)
                {
                    // ICallbackを1.0fまで進める。
                    _callback.Progress(1.0f);
                    _currentProgress = float.MaxValue;
                }

                // 前回実行時からの経過が100ms以上だった場合はProgressする。
                long curTime = DateTime.Now.Ticks;
                if (curTime - _lastUpdate > MS_100)
                {
                    // 経過時間に対して進捗割合とステップ数を計算してProgressする。
                    _callback.Progress((float)ratio);
                    float step = Step();
                    double nsteps = ratio / step;
                    _currentProgress += (float)(Math.Floor(nsteps) * step);

                    // 計算に失敗してratioが_currentProgressを超えることがあるので確認する。
                    System.Diagnostics.Debug.Assert(ratio < _currentProgress);

                    _lastUpdate = curTime;
                }
            }
        }

        /// <summary>
        /// ICallbackを完了させる。
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="time">The time.</param>
        public void EndBlock(long size, TimeSpan time)
        {
            _callback.End(size, time);
        }

        /// <summary>
        /// Progressの種類を表すEnum。
        /// </summary>
        public enum Kind
        {
            Input,
            Output
        }

        /// <summary>
        /// Progress callback
        /// </summary>
        public interface ICallback
        {
            /// <summary>
            /// This function will be called for each step at the beginning.
            /// </summary>
            /// <param name="name">step name</param>
            /// <param name="kind"></param>
            void Start(string name, Kind kind);

            /// <summary>
            /// This function will be called as progress is happening.
            /// </summary>
            /// <param name="progress">ratio of the progress</param>
            void Progress(float progress);

            /// <summary>
            /// End
            /// </summary>
            /// <param name="size">The size.</param>
            /// <param name="time">The time.</param>
            void End(long size, TimeSpan time);
        }
    }
}
