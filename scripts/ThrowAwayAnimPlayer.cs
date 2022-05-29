using Godot;
using System;
using Utils;

public class ThrowAwayAnimPlayer : Node
{
    public override void _Process(float delta)
    {
        if ((_posAnim != null) && (_mainNode != null))
        {
            _posAnim.Process(delta);
            _scaleAnim.Process(delta);
            _mainNode.Position = _posAnim.GetCurrentState();
            float scl = _scaleAnim.GetCurrentState();
            _mainNode.Scale = new Vector2(scl, scl);

            if (_posAnim.Finished) _mainNode.QueueFree();
            return;
        }
    }

    private Anim<Vector2> _posAnim;
    private Anim<float> _rotAnim;
    private Anim<float> _scaleAnim;
    public void PlayDestroyAnimation(Vector2 start, Vector2 binPos)
    {
        _posAnim = new Anim<Vector2>(new AnimPhases<Vector2>(start,
            new Vector2Anim(AnimationPhase<Vector2>.CurveType.Linear, Controls.Singleton.ThrowInBinAnimLength, binPos)));
        // _rotAnim = 
        _scaleAnim = new Anim<float>(new AnimPhases<float>(_mainNode.Scale.x,
            new FloatAnim(AnimationPhase<float>.CurveType.Linear, Controls.Singleton.ThrowInBinAnimLength, 0)));
    }

    private Node2D _mainNode;
    public void SetMainNode (Node2D node)
    {
        _mainNode = node;
    }
}
