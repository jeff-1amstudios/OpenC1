using System;
using System.Collections.Generic;
using System.Text;

namespace Carmageddon.Parsers.Funks
{

    public enum FunkActivationType
    {
        Constant,
        Distance
    }

    
    public enum FunkLoopType
    {
        None,
        Continuous
    }

    abstract class BaseFunk
    {
        public string MaterialName { get; set; }
        public CMaterial Material { get; set; }
        public FunkActivationType Activation { get; set; }

        public virtual void BeforeRender() {}
        public virtual void AfterRender() { }
        public abstract void Update();
    }
}
