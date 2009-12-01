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

        public BaseGroove ReadFromFile(BaseTextFile file)
        {
            string actorName = file.ReadLine();

            if (actorName == "FLWHEEL.ACT" || actorName == "FRWHEEL.ACT" ||
                actorName == "RLWHEEL.ACT" || actorName == "RRWHEEL.ACT" ||
                actorName == "FRPIVOT.ACT" || actorName == "FLPIVOT.ACT")
            {
                ReadToEndOfGroove(file);
                return null;
            }

            string lollipop = file.ReadLine().ToUpper();
            Debug.Assert(lollipop.StartsWith("NOT A"));
            string movement = file.ReadLine(); //constant / distance
            string path = file.ReadLine().ToUpper(); //no path
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
                ReadToEndOfGroove(file);
                return rock;
            }
            else
            {
                ReadToEndOfGroove(file);
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
