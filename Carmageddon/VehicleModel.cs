using System;
using System.Collections.Generic;
using System.Text;
using Carmageddon.Parsers;
using Carmageddon.Parsers.Grooves;
using Carmageddon.Parsers.Funks;
using Microsoft.Xna.Framework;
using System.IO;

namespace Carmageddon
{
    class VehicleModel
    {
        CActorHierarchy _actors;
        List<BaseGroove> _grooves;
        CarFile Config;

        public VehicleModel(CarFile file, bool forDisplayOnly)
        {
            Config = file;
            
            foreach (string pixFileName in file.PixFiles)
            {
                PixFile pixFile = new PixFile(GameVars.BasePath + "data\\pixelmap\\" + pixFileName);
                ResourceCache.Add(pixFile);
            }

            foreach (string matFileName in file.MaterialFiles)
            {
                MatFile matFile = new MatFile(GameVars.BasePath + "data\\material\\" + matFileName);
                ResourceCache.Add(matFile);
            }

            foreach (string matFileName in file.CrashMaterialFiles)
            {
                string filename = GameVars.BasePath + "data\\material\\" + matFileName;
                if (File.Exists(filename))
                {
                    MatFile matFile = new MatFile(GameVars.BasePath + "data\\material\\" + matFileName);
                    ResourceCache.Add(matFile);
                }
            }

            ResourceCache.ResolveMaterials();

            _grooves = new List<BaseGroove>();
            foreach (BaseGroove g in file.Grooves)
                if (!g.IsWheelActor) _grooves.Add(g);
            
            DatFile modelFile = new DatFile(GameVars.BasePath + "data\\models\\" + file.ModelFile, !forDisplayOnly);
            ActFile actFile = new ActFile(GameVars.BasePath + "data\\actors\\" + file.ActorFile, modelFile.Models);

            _actors = actFile.Hierarchy;
            _actors.ResolveTransforms(!forDisplayOnly, _grooves);
            
            foreach (BaseGroove g in _grooves)
                g.SetActor(_actors.GetByName(g.ActorName));

            // link the funks and materials
            foreach (BaseFunk f in file.Funks)
                f.Resolve();

            Vector3 tireWidth = new Vector3(0.034f, 0, 0) * GameVars.Scale;

            foreach (int id in file.DrivenWheelRefs)
            {
                BaseGroove g = file.Grooves.Find(a => a.Id == id);
                if (g == null) continue;
                CActor actor = _actors.GetByName(g.ActorName);
                CWheelActor ca = new CWheelActor(actor, true, false);
                ca.Position = actor.Matrix.Translation + (ca.IsLeft ? -1 * tireWidth : tireWidth);
                file.WheelActors.Add(ca);
            }
            foreach (int id in file.NonDrivenWheelRefs)
            {
                BaseGroove g = file.Grooves.Find(a => a.Id == id);
                CActor actor = _actors.GetByName(g.ActorName);
                if (actor == null) continue;  //BUSTER.TXT does some weird shit for cockpit view of the front wheels
                CWheelActor ca = new CWheelActor(actor, false, true);
                ca.Position = actor.Matrix.Translation + (ca.IsLeft ? -1 * tireWidth : tireWidth);
                file.WheelActors.Add(ca);
            }

            if (forDisplayOnly) _actors.RenderWheelsSeparately = false;
        }

        public void Update()
        {
            foreach (BaseGroove groove in _grooves)
            {
                groove.Update();
            }

            foreach (BaseFunk funk in Config.Funks)
            {
                funk.Update();
            }
        }

        public void Render(Matrix pose)
        {
            _actors.Render(pose, null);
        }

        public void RenderSinglePart(CActor actor)
        {
            _actors.RenderSingle(actor);
        }

        public CActor GetActor(string name)
        {
            return _actors.GetByName(name);
        }
    }
}
