using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Carmageddon
{
    static class GridPlacer
    {
        private static int _number;

        public static void Reset()
        {
            _number = 0;
        }

        public static Matrix GetGridPosition(Vector3 gridPosition, float gridDirection)
        {
            Vector3 offset = new Vector3();
            offset.X = _number % 2 == 0 ? 0 : -6f;
            offset.Z = (_number / 2) * 12f;

            Matrix m = Matrix.CreateTranslation(offset) * Matrix.CreateRotationY(gridDirection) * Matrix.CreateTranslation(gridPosition);
            
            _number++;
            return m;
        }
    }
}
