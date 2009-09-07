using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Carmageddon.Parsers
{
    public interface IFunk
    {
        void Update(GameTime gameTime);
    }

    public enum FunkActivationType
    {
        Constant,
        Distance
    }

    public enum FunkTextureAnimationType
    {
        None,
        Roll,
        Slither,
        Throb,
        Frames,
        Flic
    }

    public enum FunkLoopType
    {
        None,
        Continuous
    }

    public enum FunkType
    {
        Flic,
        Frames
    }

    class BaseFunk
    {
        public string MaterialName { get; set; }
        public FunkActivationType Activation { get; set; }
        public FunkTextureAnimationType TextureAnimationType { get; set; }
        public IFunk Funk { get; private set; }

        public BaseFunk(BaseTextFile reader, string materialName)
        {
            //string line = reader.ReadLine();

            MaterialName = materialName; // reader.ReadLine();
            string flag = reader.ReadLine();
            Activation = Helpers.TryParse<FunkActivationType>(flag, true);
            flag = reader.ReadLine();
            TextureAnimationType = Helpers.TryParse<FunkTextureAnimationType>(flag, true);

            if (TextureAnimationType == FunkTextureAnimationType.None)
            {
                reader.ReadLine();
                flag = reader.ReadLine();
                TextureAnimationType = Helpers.TryParse<FunkTextureAnimationType>(flag, true);
            }

            Debug.WriteLine("Track funk: " + MaterialName + ", " + TextureAnimationType);


            switch (TextureAnimationType)
            {
                case FunkTextureAnimationType.Roll:
                    Funk = new RollFunk(reader);
                    break;
                case FunkTextureAnimationType.Throb:
                    Funk = new ThrobFunk(reader);
                    break;
                case FunkTextureAnimationType.Slither:
                    Funk = new SlitherFunk(reader);
                    break;
                case FunkTextureAnimationType.Frames:
                    Funk = new AnimatedFramesFunk(reader);
                    break;
                case FunkTextureAnimationType.Flic:
                    Funk = new FlicFunk(reader);
                    break;

                default:
                    break;
            }
        }
    }

    class RollFunk : IFunk
    {
        public FunkLoopType Loop { get; set; }
        public Vector2 LoopSpeed { get; private set; }

        public RollFunk(BaseTextFile reader)
        {
            string flag = reader.ReadLine();
            Loop = Helpers.TryParse<FunkLoopType>(flag, true);
            LoopSpeed = reader.ReadLineAsVector2(false);

            reader.ReadLine();
            reader.ReadLine();
        }

        public void Update(GameTime gameTime)
        {

        }
    }

    class SlitherFunk : IFunk
    {
        string type;
        public Vector2 Speed { get; private set; }
        public Vector2 MoveDistance {get; private set; }

        public SlitherFunk(BaseTextFile reader)
        {
            type = reader.ReadLine();
            Speed = reader.ReadLineAsVector2(false);
            MoveDistance = reader.ReadLineAsVector2(false);

            reader.ReadLine();
            reader.ReadLine();
        }

        public void Update(GameTime gameTime)
        {

        }
    }

    class ThrobFunk : IFunk
    {
        string type;
        public FunkLoopType Loop { get; set; }
        public Vector2 Speed { get; private set; }
        public Vector2 MinSize { get; private set; }
        public Vector2 MaxSize { get; private set; }

        public ThrobFunk(BaseTextFile reader)
        {
            type = reader.ReadLine();
            Speed = reader.ReadLineAsVector2(false);
            MinSize = reader.ReadLineAsVector2(false);
            MaxSize = reader.ReadLineAsVector2(false);

            reader.ReadLine();
            reader.ReadLine();
        }

        public void Update(GameTime gameTime)
        {

        }
    }

    class AnimatedFramesFunk : IFunk
    {
        FunkLoopType Loop { get; set; }
        public List<string> FrameNames {get; private set; }

        public AnimatedFramesFunk(BaseTextFile reader)
        {
            FrameNames = new List<string>();

            reader.ReadLine(); //approximate/accurate

            string flag = reader.ReadLine();
            Loop = Helpers.TryParse<FunkLoopType>(flag, true);
            
            int unk = reader.ReadLineAsInt();
            int frames = reader.ReadLineAsInt();
            for (int i = 0; i < frames; i++)
            {
                FrameNames.Add(reader.ReadLine());
            }
        }

        public void Update(GameTime gameTime)
        {
            
        }
    }

    class FlicFunk : IFunk
    {
        public FunkLoopType Loop { get; private set; }
        public string FliName { get; private set; }

        public FlicFunk(BaseTextFile reader)
        {
            reader.ReadLine(); //approximate/accurate
            FliName = reader.ReadLine();
        }

        public void Update(GameTime gameTime)
        {

        }
    }
}


