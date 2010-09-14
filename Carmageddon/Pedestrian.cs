using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon
{

    class Pedestrian
    {
        public int RefNumber;
        public int InitialInstruction;
        public PedestrianBehaviour Behaviour;
        public List<IPedestrianInstruction> Instructions = new List<IPedestrianInstruction>();

        PedestrianAction _currentAction;
        PedestrianSequence _currentSequence;

        private bool _inLoopingFrames;
        private int _frameIndex;
        private float _frameTime;
        public Vector3 Position;

        public Pedestrian()
        {
        }

        public void Initialize()
        {
            SetAction(Behaviour.Standing);
            Position = ((PedestrianPointInstruction)Instructions[InitialInstruction]).Position;
        }

        public void SetAction(PedestrianAction action)
        {
            if (_currentAction == action) return;

            _currentAction = action;
            _currentSequence = Behaviour.Sequences[_currentAction.Sequences[0].SequenceIndex];
            _frameIndex = 0;
            _frameTime = 0;
            _inLoopingFrames = (_currentSequence.InitialFrames.Count == 0);  //if no initialframes, jump straight to looping frames
        }

        public void Update()
        {
            _frameTime += Engine.ElapsedSeconds;
            if (_frameTime > 0.5f)
            {
                _frameIndex++;
                _frameTime = 0;
            }

            if (!_inLoopingFrames)
            {
                if (_frameIndex >= _currentSequence.InitialFrames.Count)
                {
                    _frameIndex = 0;
                    _frameTime = 0;
                    if (_currentSequence.LoopingFrames.Count > 0) _inLoopingFrames = true;
                }
            }
            else
            {
                if (_frameIndex >= _currentSequence.LoopingFrames.Count)
                {
                    _frameIndex = 0;
                    _frameTime = 0;
                }
            }
        }

        public void Render()
        {
            Matrix world = Matrix.CreateScale(0.015f) * Matrix.CreateBillboard(Position, Engine.Camera.Position, Vector3.Up, Vector3.Forward);

            PedestrianFrame frame = _inLoopingFrames ? _currentSequence.LoopingFrames[_frameIndex] : _currentSequence.InitialFrames[_frameIndex];
            Vector3 texSize = new Vector3(1, 1, 1);
            if (frame.Texture != null) texSize = new Vector3(frame.Texture.Width, frame.Texture.Height, 1);
            Vector3 scale = new Vector3(1);
            Matrix scaleMatrix = Matrix.CreateScale(scale * texSize);

            if (frame.Flipped)
            {
                scaleMatrix = scaleMatrix * Matrix.CreateRotationY(MathHelper.Pi);
            }

            BasicEffect2 effect = GameVars.CurrentEffect;
            effect.World = scaleMatrix * world;
            effect.Texture = frame.Texture;
            effect.CommitChanges();
            Engine.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }
    }

    interface IPedestrianInstruction
    {
    }
    class PedestrianPointInstruction : IPedestrianInstruction
    {
        public Vector3 Position;
    }
    class PedestrianReverseInstruction : IPedestrianInstruction
    {
    }
}
