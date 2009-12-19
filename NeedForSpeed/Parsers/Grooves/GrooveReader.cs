using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Carmageddon.Parsers.Grooves
{
    
    class GrooveReader
    {
        public bool AtEnd;

        public BaseGroove Read(BaseTextFile file)
        {
            string actorName = file.ReadLine();
            if (actorName == "END OF GROOVE")
            {
                AtEnd = true;
                return null;
            }

            if (actorName.StartsWith("FLWHEEL") || actorName.StartsWith("FRWHEEL") ||
                actorName.StartsWith("RLWHEEL") || actorName.StartsWith("RRWHEEL") ||
                actorName.StartsWith("FRPIVOT") || actorName.StartsWith("FLPIVOT"))
            {
                ReadToEndOfGroove(file);
                return null;
            }

            string lollipop = file.ReadLine().ToUpper();
            Debug.Assert(lollipop.StartsWith("NOT A"));
            string movement = file.ReadLine(); //constant / distance
            string pathType = file.ReadLine().ToUpper(); //no path
            PathGroove path = null;
            if (pathType == "STRAIGHT")
            {
                path = new PathGroove();
                path.Motion = (Motion)Enum.Parse(typeof(Motion), file.ReadLine(), true);
                path.CenterOfMovement = file.ReadLineAsVector3(false);
                path.Speed = file.ReadLineAsFloat(false);
                path.Movement = file.ReadLineAsVector3(false);
            }
            //Debug.Assert(path.StartsWith("NO "));
            string action = file.ReadLine().ToUpper();

            if (action == "SPIN")
            {
                SpinGroove spin = new SpinGroove();

                spin.ActorName = actorName;
                string actionTrigger = file.ReadLine(); //continuous, controlled
                spin.Speed = file.ReadLineAsFloat(false);
                spin.CenterOfMovement = file.ReadLineAsVector3(false);
                spin.Axis = (Axis)Enum.Parse(typeof(Axis), file.ReadLine(), true);
                spin.Path = path;
                ReadToEndOfGroove(file);
                return spin;
            }
            else if (action == "ROCK")
            {
                RockGroove rock = new RockGroove();
                rock.ActorName = actorName;
                rock.Motion = (Motion)Enum.Parse(typeof(Motion), file.ReadLine(), true);
                rock.Speed = file.ReadLineAsFloat(false);
                rock.CenterOfMovement = file.ReadLineAsVector3(false);
                rock.Axis = (Axis)Enum.Parse(typeof(Axis), file.ReadLine(), true);
                rock.MaxAngle = MathHelper.ToRadians(file.ReadLineAsFloat(false));  //in degrees in file
                rock.Path = path;
                ReadToEndOfGroove(file);
                return rock;
            }
            else
            {
                ReadToEndOfGroove(file);
                if (path != null)
                {
                    path.ActorName = actorName;
                    return path;
                }
                return null;
            }
        }

        private void ReadToEndOfGroove(BaseTextFile file)
        {
            while (true)
            {
                string line = file.ReadLine();
                if (line == "NEXT GROOVE")
                    return;
                if (line == "END OF GROOVE")
                {
                    AtEnd = true;
                    return;
                }
            }
        }
    }
}
