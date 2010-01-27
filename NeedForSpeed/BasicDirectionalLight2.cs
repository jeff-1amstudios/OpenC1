using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Carmageddon
{
    public sealed class BasicDirectionalLight2
    {
        // Fields
        private Vector3 cachedDiffuseColor;
        private Vector3 cachedSpecularColor;
        private EffectParameter diffuseColorParam;
        private EffectParameter directionParam;
        private bool enabled;
        private EffectParameter specularColorParam;

        // Methods
        internal BasicDirectionalLight2(EffectParameter direction, EffectParameter diffuseColor, EffectParameter specularColor)
        {
            this.directionParam = direction;
            this.diffuseColorParam = diffuseColor;
            this.specularColorParam = specularColor;
        }

        internal void Copy(BasicDirectionalLight2 from)
        {
            this.enabled = from.enabled;
            this.cachedDiffuseColor = from.cachedDiffuseColor;
            this.cachedSpecularColor = from.cachedSpecularColor;
            this.diffuseColorParam.SetValue(this.cachedDiffuseColor);
            this.specularColorParam.SetValue(this.cachedSpecularColor);
        }

        // Properties
        public Vector3 DiffuseColor
        {
            get
            {
                return this.cachedDiffuseColor;
            }
            set
            {
                if (this.enabled)
                {
                    this.diffuseColorParam.SetValue(value);
                }
                this.cachedDiffuseColor = value;
            }
        }

        public Vector3 Direction
        {
            get
            {
                return this.directionParam.GetValueVector3();
            }
            set
            {
                this.directionParam.SetValue(value);
            }
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                if (this.enabled != value)
                {
                    this.enabled = value;
                    if (this.enabled)
                    {
                        this.diffuseColorParam.SetValue(this.cachedDiffuseColor);
                        this.specularColorParam.SetValue(this.cachedSpecularColor);
                    }
                    else
                    {
                        this.diffuseColorParam.SetValue(Vector3.Zero);
                        this.specularColorParam.SetValue(Vector3.Zero);
                    }
                }
            }
        }

        public Vector3 SpecularColor
        {
            get
            {
                return this.cachedSpecularColor;
            }
            set
            {
                if (this.enabled)
                {
                    this.specularColorParam.SetValue(value);
                }
                this.cachedSpecularColor = value;
            }
        }
    }
}