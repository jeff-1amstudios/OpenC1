using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StillDesign.PhysX;
using System;
using PlatformEngine;

namespace Carmageddon.Physics
{
    
    internal class PhysX
    {
        private static PhysX _instance;

        public StillDesign.PhysX.Core Core { get; private set; }
        public StillDesign.PhysX.Scene Scene { get; private set; }

        private BasicEffect _debugEffect;

        public static PhysX Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PhysX();
                return _instance;
            }
        }

        public float Gravity
        {
            get { return -9.81f * 1.1f; }  //gravity + a bit extra
        }

        private PhysX()
        {
            try
            {
                Core = new StillDesign.PhysX.Core();
            }
            catch (Exception exception)
            {
                //ScreenManager.Graphics.IsFullScreen = false;
                //ScreenManager.Graphics.ApplyChanges();
                
                //MessageBox.Show("Error initializing PhysX.\n- Did you install the nVidia PhysX System Software?\n\n" + exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }

            //Core.SetParameter(PhysicsParameter.SkinWidth, (float)0.01f);
            Core.SetParameter(PhysicsParameter.VisualizationScale, (float)0f);
            Core.SetParameter(PhysicsParameter.ContinuousCollisionDetection, false);
            Core.SetParameter(PhysicsParameter.VisualizeCollisionShapes, false);
            Core.SetParameter(PhysicsParameter.VisualizeContactNormal, true);
            Core.SetParameter(PhysicsParameter.VisualizeContactForce, true);
            Core.SetParameter(PhysicsParameter.VisualizeActorAxes, false);
            
            
            SceneDescription sceneDescription = new SceneDescription();
            sceneDescription.Gravity = new Vector3(0f, Gravity, 0f);
            sceneDescription.TimestepMethod = TimestepMethod.Fixed;

            sceneDescription.Flags = SceneFlag.SimulateSeparateThread; // SceneFlag.SequentialPrimart;
            //sceneDescription.InternalThreadCount = 1; // HexaChromeGame.ProcessorCount - 1;
            sceneDescription.ThreadMask = 0xfffffffe;
            Scene = Core.CreateScene(sceneDescription);

            Scene.UserContactReport = ContactReport.Instance;
            Scene.UserTriggerReport = TriggerReport.Instance;
            
            MaterialDescription description = new MaterialDescription();
            description.Restitution = 0.1f;
            description.DynamicFriction = 0.2f;
            Scene.DefaultMaterial.LoadFromDescription(description);
            InitScene();
        }

        public void Delete()
        {
            Scene.ShutdownWorkerThreads();
            Scene.Dispose();
            Core.Dispose();
            Scene = null;
            Core = null;
        }

        public void Draw()
        {
            if (_debugEffect == null)
            {
                _debugEffect = new BasicEffect(Engine.Device, null);
            }

            _debugEffect.View = Engine.Camera.View;
            _debugEffect.World = Matrix.Identity;
            _debugEffect.Projection = Engine.Camera.Projection;;
            DebugRenderable debugRenderable = Scene.GetDebugRenderable();
            Engine.Device.VertexDeclaration = new VertexDeclaration(Engine.Device, VertexPositionColor.VertexElements);
            _debugEffect.Begin();

            foreach (EffectPass pass in _debugEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                if (debugRenderable.PointCount > 0)
                {
                    DebugPoint[] debugPoints = debugRenderable.GetDebugPoints();
                    Engine.Device.DrawUserPrimitives<DebugPoint>(PrimitiveType.PointList, debugPoints, 0, debugPoints.Length);
                }
                if (debugRenderable.LineCount > 0)
                {
                    DebugLine[] debugLines = debugRenderable.GetDebugLines();
                    VertexPositionColor[] vertexData = new VertexPositionColor[debugRenderable.LineCount * 2];
                    for (int i = 0; i < debugRenderable.LineCount; i++)
                    {
                        DebugLine line = debugLines[i];
                        vertexData[i * 2] = new VertexPositionColor(line.Point0, Color.White);
                        vertexData[(i * 2) + 1] = new VertexPositionColor(line.Point1, Color.White);
                    }
                    Engine.Device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertexData, 0, debugLines.Length);
                }
                if (debugRenderable.TriangleCount > 0)
                {
                    DebugTriangle[] debugTriangles = debugRenderable.GetDebugTriangles();
                    VertexPositionColor[] colorArray2 = new VertexPositionColor[debugRenderable.TriangleCount * 3];
                    for (int j = 0; j < debugRenderable.TriangleCount; j++)
                    {
                        DebugTriangle triangle = debugTriangles[j];
                        colorArray2[j * 3] = new VertexPositionColor(triangle.Point0, Color.White);
                        colorArray2[(j * 3) + 1] = new VertexPositionColor(triangle.Point1, Color.White);
                        colorArray2[(j * 3) + 2] = new VertexPositionColor(triangle.Point2, Color.White);
                    }
                    Engine.Device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, colorArray2, 0, debugTriangles.Length);
                }
                pass.End();
            }
            _debugEffect.End();
        }

        private void InitScene()
        {
        }

        public void Simulate()
        {
            Scene.Simulate(Engine.ElapsedSeconds);
            Scene.FlushStream();            
        }

        public void Fetch()
        {
            Scene.FetchResults(SimulationStatus.AllFinished, true);
        }
    }
}

