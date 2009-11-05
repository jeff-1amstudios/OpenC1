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

        private const float time = 0.01666667f;
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

            Core.SetParameter(PhysicsParameter.SkinWidth, (float)0.01f);
            Core.SetParameter(PhysicsParameter.VisualizationScale, (float)1f);
            Core.SetParameter(PhysicsParameter.ContinuousCollisionDetection, false);
            Core.SetParameter(PhysicsParameter.VisualizeCollisionShapes, true);
            
            SceneDescription sceneDescription = new SceneDescription();
            sceneDescription.Gravity = new Vector3(0f, -9.81f*1.5f, 0f);  //double gravity
            sceneDescription.TimestepMethod = TimestepMethod.Fixed;
            
            sceneDescription.Flags = SceneFlag.EnableMultithread | SceneFlag.SimulateSeperateThread;
            //sceneDescription.InternalThreadCount = 1; // HexaChromeGame.ProcessorCount - 1;
            sceneDescription.ThreadMask = 0xfffffffe;
            Scene = Core.CreateScene(sceneDescription);
            //Scene.SetGroupCollisionFlag(ds.HeightfieldGroupID, ds.VehicleGroupID, true);
            //Scene.SetActorGroupPairFlags(ds.HeightfieldGroupID, ds.VehicleGroupID, ContactPairFlag.OnStartTouch);
            //Scene.SetGroupCollisionFlag(ds.HeightfieldGroupID, ds.RocketGroupID, true);
            //Scene.SetActorGroupPairFlags(ds.HeightfieldGroupID, ds.RocketGroupID, ContactPairFlag.OnImpact | ContactPairFlag.OnTouch | ContactPairFlag.OnStartTouch);
            //Scene.SetGroupCollisionFlag(ds.VehicleGroupID, ds.RocketGroupID, true);
            //Scene.SetActorGroupPairFlags(ds.VehicleGroupID, ds.RocketGroupID, ContactPairFlag.OnTouch | ContactPairFlag.OnStartTouch);
            //Scene.SetGroupCollisionFlag(ds.VehicleGroupID, ds.ItemGroupID, true);
            //Scene.SetActorGroupPairFlags(ds.VehicleGroupID, ds.ItemGroupID, ContactPairFlag.OnTouch | ContactPairFlag.OnStartTouch);
            //Scene.UserContactReport = new ContactReport(Scene);
            
            MaterialDescription description = new MaterialDescription();
            description.Restitution = 0.4f;
            description.StaticFriction = 0.2f;
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
                _debugEffect = new BasicEffect(Engine.Instance.Device, null);
            }

            _debugEffect.View = Engine.Instance.Camera.View;
            _debugEffect.World = Matrix.Identity;
            _debugEffect.Projection = Engine.Instance.Camera.Projection;;
            DebugRenderable debugRenderable = Scene.GetDebugRenderable();
            Engine.Instance.Device.VertexDeclaration = new VertexDeclaration(Engine.Instance.Device, VertexPositionColor.VertexElements);
            _debugEffect.Begin();

            foreach (EffectPass pass in _debugEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                if (debugRenderable.PointCount > 0)
                {
                    DebugPoint[] debugPoints = debugRenderable.GetDebugPoints();
                    Engine.Instance.Device.DrawUserPrimitives<DebugPoint>(PrimitiveType.PointList, debugPoints, 0, debugPoints.Length);
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
                    Engine.Instance.Device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertexData, 0, debugLines.Length);
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
                    Engine.Instance.Device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, colorArray2, 0, debugTriangles.Length);
                }
                pass.End();
            }
            _debugEffect.End();
        }

        private void InitScene()
        {
        }

        public void Update(GameTime gameTime)
        {
            Scene.Simulate((float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0));
            Scene.FlushStream();
            Scene.FetchResults(SimulationStatus.RigidBodyFinished, true);
        }
    }
}

