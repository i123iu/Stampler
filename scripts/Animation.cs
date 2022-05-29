using Godot;
using System;

namespace Utils
{
    public abstract class AnimationPhase<T>
    {
        public enum CurveType
        {
            Constant, Linear,
        }

        private readonly float _length;
        protected readonly CurveType _curveType;


        private readonly T _endState;
        public T GetEndState => _endState;

        public float Length => _length;


        public AnimationPhase(CurveType curveType, float length, T endState)
        {
            _curveType = curveType;
            _length = length;
            _endState = endState;
        }

        public T GetCurrentState(float time, T startState)
        {
            float progress = time / _length;
            if ((progress < -0.01f) || (progress > 1.01f)) throw new Exception("fgaeds");
            progress = Mathf.Clamp(progress, 0, 1);
            return GetState(progress, startState, _endState);
        }

        /// <param name="progress"> Between 0 (start) and (end) </param>
        protected abstract T GetState(float progress, T start, T end);
    }

    public class Vector3Anim : AnimationPhase<Vector3>
    {
        public Vector3Anim(CurveType curveType, float length, Vector3 endState) : base(curveType, length, endState)
        { }

        protected override Vector3 GetState(float progress, Vector3 start, Vector3 end)
        {
            switch (_curveType)
            {
                case CurveType.Constant:
                    return (progress >= 0.5f) ? end : start;

                case CurveType.Linear:

                    Vector3 diff = end - start;
                    diff *= progress;
                    return start + diff;

                default: throw new NotImplementedException();
            }
        }
    }
    public class Vector2Anim : AnimationPhase<Vector2>
    {
        public Vector2Anim(CurveType curveType, float length, Vector2 endState) : base(curveType, length, endState)
        { }

        protected override Vector2 GetState(float progress, Vector2 start, Vector2 end)
        {
            switch (_curveType)
            {
                case CurveType.Constant:
                    return (progress >= 0.5f) ? end : start;

                case CurveType.Linear:

                    Vector2 diff = end - start;
                    diff *= progress;
                    return start + diff;

                default: throw new NotImplementedException();
            }
        }
    }
    public class FloatAnim : AnimationPhase<float>
    {
        public FloatAnim(CurveType curveType, float length, float endState) : base(curveType, length, endState)
        { }

        protected override float GetState(float progress, float start, float end)
        {
            switch (_curveType)
            {
                case CurveType.Constant:
                    return (progress >= 0.5f) ? end : start;

                case CurveType.Linear:

                    return (end - start) * progress + start;

                default: throw new NotImplementedException();
            }
        }
    }

    public class AnimPhases<T>
    {
        private readonly T _startState;
        private readonly AnimationPhase<T>[] _phases;
        private float _totalLength;

        public AnimPhases(T startState, params AnimationPhase<T>[] phases)
        {
            _startState = startState;

            _phases = phases;

            _totalLength = 0;
            for (int i = 0; i < _phases.Length; i++)
                _totalLength += _phases[i].Length;
        }

        public int NoPhases => _phases.Length;

        public float TotalAnimLength => _totalLength;

        public T GetStartState => _startState;

        public AnimationPhase<T> this[int i]
            => _phases[i];
    }

    public class Anim<T>
    {
        private float _currAnimTime = 0;
        private int _currPhaseIdx = 0;

        private AnimPhases<T> _phases;

        public bool Finished => _currPhaseIdx == _phases.NoPhases;
        public int GetPhaseIdx => _currPhaseIdx;

        public Anim(AnimPhases<T> phases)
        {
            _phases = phases;
        }

        /// <summary>
        /// Returns true if the anim index was changed
        /// </summary>
        public bool Process(float delta)
        {
            if (Finished) return false;
            bool changed = false;

            _currAnimTime += delta;
            //GD.Print("Added ", delta, " to total = ", _currAnimTime);

            while (_currAnimTime > _phases[_currPhaseIdx].Length)
            {
                float a = _currAnimTime;
                _currAnimTime -= _phases[_currPhaseIdx].Length;

                _currPhaseIdx++;
                changed = true;

                if (Finished) break;
            }

            return changed;
        }

        public T GetCurrentState()
        {
            if (Finished)
                return _phases[_phases.NoPhases - 1].GetEndState;

            T lastEndState = (_currPhaseIdx == 0) ? _phases.GetStartState : _phases[_currPhaseIdx - 1].GetEndState;

            float progressTime = _currAnimTime;
            return _phases[_currPhaseIdx].GetCurrentState(progressTime, lastEndState);
        }

        /// <summary>
        /// Sets the animation to the end state
        /// </summary>
        public void SetFinished()
        {
            _currPhaseIdx = _phases.NoPhases;
        }

        public void Reset()
        {
            _currAnimTime = 0;
            _currPhaseIdx = 0;
        }
        public void Reset(AnimPhases<T> newPhases)
        {
            _phases = newPhases;

            Reset();
        }
    }
}
