using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using AutoPilotX.Utils;

namespace AutoPilotX.Services
{
    public class MouseMovementService
    {
        private readonly SettingsService _settings;
        private readonly Random _random = new Random();
        private readonly IInputSimulator _inputSim;

        public MouseMovementService(SettingsService settings, IInputSimulator inputSim)
        {
            _settings = settings;
            _inputSim = inputSim;
        }

        public void MoveMouse(int targetX, int targetY)
        {
            // If feature disabled, instant move
            if (!_settings.Settings.HumanLikeMouseMovement)
            {
                _inputSim.MoveMouse(targetX, targetY);
                return;
            }

            // Smooth Bezier Movement
            var start = Cursor.Position;
            var end = new Point(targetX, targetY);

            // Calculate control points for Bezier curve
            // We add some randomness to make it look human
            var distance = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
            
            // Steps relative to distance (more steps for longer distance)
            // Example: 1 step per 10 pixels?
            int steps = (int)(distance / 10);
            if (steps < 5) steps = 5;
            if (steps > 100) steps = 100;

            // Randomized Control Point
            // A simple Quadratic Bezier has 1 control point. Cubic has 2.
            // Let's use Cubic for "S" curves.
            
            var control1 = new Point(
                start.X + (end.X - start.X) / 3 + _random.Next(-50, 50),
                start.Y + (end.Y - start.Y) / 3 + _random.Next(-50, 50)
            );
            
            var control2 = new Point(
                start.X + 2 * (end.X - start.X) / 3 + _random.Next(-50, 50),
                start.Y + 2 * (end.Y - start.Y) / 3 + _random.Next(-50, 50)
            );

            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                
                // Cubic Bezier Formula
                // B(t) = (1-t)^3 * P0 + 3(1-t)^2 * t * P1 + 3(1-t) * t^2 * P2 + t^3 * P3
                
                double u = 1 - t;
                double tt = t * t;
                double uu = u * u;
                double uuu = uu * u;
                double ttt = tt * t;
                
                double x = uuu * start.X + 3 * uu * t * control1.X + 3 * u * tt * control2.X + ttt * end.X;
                double y = uuu * start.Y + 3 * uu * t * control1.Y + 3 * u * tt * control2.Y + ttt * end.Y;
                
                _inputSim.MoveMouse((int)x, (int)y);
                
                // Variable sleep for "human" speed variation (accelerate/decelerate?)
                // Simple: 1-3ms sleep
                Thread.Sleep(_random.Next(1, 3));
            }
            
            // Ensure final position is exact
            _inputSim.MoveMouse(targetX, targetY);
        }
    }
}
