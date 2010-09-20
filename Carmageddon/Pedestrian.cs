using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Microsoft.Xna.Framework.Graphics;
using StillDesign.PhysX;
using Carmageddon.Physics;
using NFSEngine;

namespace Carmageddon
{

    class Pedestrian
    {
        public static float RunningSpeed = 0.1f;

        public int RefNumber;
        public int InitialInstruction;
        public PedestrianBehaviour Behaviour;
        public List<PedestrianInstruction> Instructions = new List<PedestrianInstruction>();

        PedestrianAction _currentAction;
        PedestrianSequence _currentSequence;

        private bool _inLoopingFrames;
        private int _frameIndex;
        private float _frameTime;
        private float _frameRate;
        public Vector3 Position;
        private int _currentInstruction, _instructionDirection = 1;
        private bool _isRunning;
        private Actor _physXActor;
        private Vector3 _direction;

        public Pedestrian()
        {
        }

        public void Initialize()
        {
            SetAction(Behaviour.Standing, true);
            Position = Instructions[InitialInstruction].Position;

            ActorDescription actorDesc = new ActorDescription();
            actorDesc.BodyDescription = new BodyDescription(1);
            actorDesc.BodyDescription.MassSpaceInertia = new Vector3(1, 1, 1);
            actorDesc.BodyDescription.BodyFlags |= BodyFlag.Kinematic;

            BoxShapeDescription box = new BoxShapeDescription(1.6f, 2.5f, 1.6f);
            box.Flags = ShapeFlag.TriggerOnEnter | ShapeFlag.Visualization;
            box.LocalPosition = new Vector3(0, 1, 0);
            actorDesc.Shapes.Add(box);
            _physXActor = PhysX.Instance.Scene.CreateActor(actorDesc);
            _physXActor.GlobalPosition = Position;
            _physXActor.UserData = this;
        }

        public void SetAction(PedestrianAction action, bool force)
        {
            if (RefNumber == 130)
            {
            }
            if (_currentAction == action) return;
            if (_currentSequence != null && !_currentSequence.Collide && !force) return;

            _currentAction = action;
            SetSequence(Behaviour.Sequences[_currentAction.Sequences[0].SequenceIndex]);

            if (_currentAction.Sounds.Length > 0)
                SoundCache.Play(_currentAction.Sounds[Engine.Random.Next(_currentAction.Sounds.Length)], Race.Current.PlayerVehicle, true);
        }

        public void SetRunning(bool running)
        {
            if (_isRunning == running) return;
            _isRunning = running;
            _currentInstruction = InitialInstruction + 1;
        }

        private void SetSequence(PedestrianSequence seq)
        {
            if (_currentSequence == seq) return;

            _currentSequence = seq;
            _frameIndex = 0;
            _frameTime = 0;
            _inLoopingFrames = (_currentSequence.InitialFrames.Count == 0);  //if no initialframes, jump straight to looping frames

            UpdateFrameRate();
        }

        public void Update()
        {
//            if (RefNumber != 99) return;
            if (_frameTime < 0) return;

            float angle = Helpers.GetSignedAngleBetweenVectors(Engine.Camera.Orientation, _direction, false);
            angle = MathHelper.ToDegrees(angle);
            if (angle < 0) angle += 360;

            GameConsole.WriteLine("ped angle: " + angle);

            if (angle < 90 || angle > 270)
                GameConsole.WriteLine("AWAY");
            else
                GameConsole.WriteLine("TOWARDS");
            if (angle < 0 || angle > 360)
            {
            }
            
            
            //angle = pedAngle;
            int seq = 0;
            foreach (PedestrianActionSequenceMap seqMap in _currentAction.Sequences)
            {
                if (angle < seqMap.MaxBearing)
                {
                    seq = seqMap.SequenceIndex;
                    SetSequence(Behaviour.Sequences[seqMap.SequenceIndex]);
                    break;
                }
            }


            _frameTime += Engine.ElapsedSeconds;
            if (_frameTime > _frameRate)
            {
                _frameIndex++;
                _frameTime = 0;

                if (!_inLoopingFrames)
                {
                    if (_frameIndex >= _currentSequence.InitialFrames.Count)
                    {
                        if (_currentSequence.LoopingFrames.Count > 0)
                        {
                            _inLoopingFrames = true;
                            _frameIndex = 0;
                        }
                        else
                        {
                            _frameIndex--;
                            _frameTime = -1;  //stop animating
                        }
                    }
                }
                else
                {
                    if (_frameIndex >= _currentSequence.LoopingFrames.Count)
                    {
                        _frameIndex = 0;
                        _frameTime = 0;
                        UpdateFrameRate();
                    }
                }
            }

            if (_isRunning)
            {
                Vector3 target = Instructions[_currentInstruction].Position;
                _direction = target - Position;
                _direction.Normalize();
                Position += _direction * RunningSpeed;
                _physXActor.GlobalPosition = Position;

                if (Vector3.Distance(Position, target) < 1)
                {
                    if (Instructions[_currentInstruction].Reverse)
                    {
                        _instructionDirection *= -1;
                    }
                    _currentInstruction += _instructionDirection;
                    if (_currentInstruction == -1) { _currentInstruction = 0; _instructionDirection *= -1; }                    
                }
            }
        }

        private void UpdateFrameRate()
        {
            switch (_currentSequence.FrameRateType)
            {
                case PedestrianSequenceFrameRate.Variable:
                    _frameRate = 1 / Engine.Random.Next(_currentSequence.MinFrameRate, _currentSequence.MaxFrameRate);
                    break;
                case PedestrianSequenceFrameRate.Speed:
                    _frameRate = 1 / _currentSequence.MaxFrameRate; // Engine.Random.Next(_currentSequence.MinFrameRate, _currentSequence.MaxFrameRate);
                    break;
                case PedestrianSequenceFrameRate.Fixed:
                    _frameRate = 0.1f;
                    break;
            }
        }

        public void Render()
        {
            //if (RefNumber != 99) return;

            Matrix world = Matrix.CreateConstrainedBillboard(Position, Engine.Camera.Position, Vector3.Up, null, null);

            PedestrianFrame frame = _inLoopingFrames ? _currentSequence.LoopingFrames[_frameIndex] : _currentSequence.InitialFrames[_frameIndex];
            Vector3 texSize = new Vector3(50, 70, 1);
            if (frame.Texture != null) texSize = new Vector3(frame.Texture.Width, frame.Texture.Height, 1);
            texSize /= Math.Max(texSize.X, texSize.Y);
            Vector3 scale = texSize * new Vector3(Behaviour.Height, Behaviour.Height, 1);

            if (frame.Flipped)
            {
                world = Matrix.CreateRotationY(MathHelper.Pi) * world;
            }

            world = Matrix.CreateScale(scale) * world;

            BasicEffect2 effect = GameVars.CurrentEffect;
            effect.World = world;
            effect.Texture = frame.Texture;
            effect.CommitChanges();
            Engine.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }
    }

    class PedestrianInstruction
    {
        public Vector3 Position;
        public bool Reverse;
    }
}
