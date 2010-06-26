using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PlatformEngine;
using Carmageddon.Parsers;

namespace Carmageddon
{
    class PixmapBillboard
    {
        VertexPositionTexture[] _vertices;
        VertexBuffer _vertexBuffer;
        VertexDeclaration _vertexDeclaration;

        List<PixMap> _pixmaps;
        float _currentFrameTime;
        int _currentFrame;
        Vector3 _scale;
        Matrix _scaleMatrix;

        public PixmapBillboard(Vector2 scale, string filename)
        {
            _scale = new Vector3(scale, 1);
            CreateGeometry();
            _vertexDeclaration = new VertexDeclaration(Engine.Device, VertexPositionTexture.VertexElements);

            PixFile pix = new PixFile(filename);
            _pixmaps = pix.PixMaps;
        }

        public void BeginBatch()
        {
            Engine.Device.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionTexture.SizeInBytes);
            Engine.Device.VertexDeclaration = _vertexDeclaration;
            //Engine.CurrentEffect.CurrentTechnique.Passes[0].Begin();
        }

        public void EndBatch()
        {
            //Engine.CurrentEffect.CurrentTechnique.Passes[0].End();
        }

        public void Render(Vector3 position)
        {
            Update();
            BeginBatch();

            Matrix world = Matrix.CreateScale(0.03f) * Matrix.CreateBillboard(position, Engine.Camera.Position, Vector3.Up, Vector3.Forward);

            BasicEffect2 effect = GameVariables.CurrentEffect;
            effect.World = _scaleMatrix * world;
            effect.Texture = _pixmaps[_currentFrame].Texture;
            effect.LightingEnabled = false;
            effect.CommitChanges(); 
            Engine.Device.RenderState.CullMode = CullMode.None;
            Engine.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effect.LightingEnabled = true;
            EndBatch();
        }

        private void CreateGeometry()
        {
            Vector3 topLeftFront = new Vector3(-0.5f, 1.0f, 0.5f);
            Vector3 bottomLeftFront = new Vector3(-0.5f, 0.0f, 0.5f);
            Vector3 topRightFront = new Vector3(0.5f, 1.0f, 0.5f);
            Vector3 bottomRightFront = new Vector3(0.5f, 0.0f, 0.5f);

            Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureTopRight = new Vector2(1.0f, 0.0f);
            Vector2 textureBottomLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureBottomRight = new Vector2(1.0f, 1.0f);

            _vertices = new VertexPositionTexture[4];
            _vertices[0] = new VertexPositionTexture(topLeftFront, textureTopLeft);
            _vertices[1] = new VertexPositionTexture(bottomLeftFront, textureBottomLeft);
            _vertices[2] = new VertexPositionTexture(topRightFront, textureTopRight);
            _vertices[3] = new VertexPositionTexture(bottomRightFront, textureBottomRight);

            _vertexBuffer = new VertexBuffer(Engine.Device,
                                                 VertexPositionTexture.SizeInBytes * _vertices.Length,
                                                 BufferUsage.WriteOnly);

            _vertexBuffer.SetData<VertexPositionTexture>(_vertices);
        }


        public void Update()
        {
            _currentFrameTime += Engine.ElapsedSeconds;
            if (_currentFrameTime > 0.03f)
            {
                _currentFrame++;
                if (_currentFrame == _pixmaps.Count) _currentFrame = 0;
                Vector3 texSize = new Vector3(_pixmaps[_currentFrame].Texture.Width, _pixmaps[_currentFrame].Texture.Height, 1);
                _scaleMatrix = Matrix.CreateScale(_scale * texSize);
                _currentFrameTime = 0;
            }
        }


        //public void Render(Matrix world)
        //{
        //    Update();

        //    bbEffect.CurrentTechnique = bbEffect.Techniques["CylBillboard"];
        //    bbEffect.Parameters["xWorld"].SetValue(Matrix.CreateTranslation(0, 0, 0));
        //    bbEffect.Parameters["xView"].SetValue(Engine.Camera.View);
        //    bbEffect.Parameters["xProjection"].SetValue(Engine.Camera.Projection);
        //    bbEffect.Parameters["xCamPos"].SetValue(Engine.Camera.Position);
        //    bbEffect.Parameters["xAllowedRotDir"].SetValue(new Vector3(0, 1, 0));
        //    bbEffect.Parameters["xBillboardTexture"].SetValue(_pixmaps[_currentFrame].Texture);

        //    bbEffect.Begin();
        //    foreach (EffectPass pass in bbEffect.CurrentTechnique.Passes)
        //    {
        //        pass.Begin();
        //        Engine.Device.Vertices[0].SetSource(treeVertexBuffer, 0, VertexPositionTexture.SizeInBytes);
        //        Engine.Device.VertexDeclaration = treeVertexDeclaration;
        //        int noVertices = treeVertexBuffer.SizeInBytes / VertexPositionTexture.SizeInBytes;
        //        int noTriangles = noVertices / 3;
        //        Engine.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, noTriangles);
        //        pass.End();
        //    }
        //    bbEffect.End();
        //}
    }


}
