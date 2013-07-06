using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using RubeLoader.Interfaces;

namespace RUBELoaderTest.Entities
{
    public class Car : IJointController, IBodyController
    {
        private const float MaxMotorSpeed = 20f;

        public string JointName { get; set; }
        public List<Joint> Joints { get; set; }

        public string BodyName { get; set; }
        public List<Body> Bodies { get; set; }

        public List<WheelJoint> Wheels
        {
            get { return Joints.Cast<WheelJoint>().ToList(); }
        }

        public void Init()
        {
            Wheels.ForEach(w => w.MotorEnabled = true);
            //Wheel.MaxMotorTorque = 150;
        }
        
        public void MoveLeft()
        {
            foreach (var wheel in Wheels)
            {
                wheel.MotorEnabled = true;
                if (wheel.MotorSpeed > -MaxMotorSpeed)
                {
                    wheel.MotorSpeed -= 1f;
                }
            }
        }

        public void MoveRight()
        {
            foreach (var wheel in Wheels)
            {
                wheel.MotorEnabled = true;
                if (wheel.MotorSpeed < MaxMotorSpeed)
                {
                    wheel.MotorSpeed += 1f;
                }
            }
        }

        public void Brake()
        {
            foreach (var wheel in Wheels)
            {
                wheel.MotorEnabled = true;
                wheel.MotorSpeed = 0f;
            }
        }

        public void Jump()
        {
            Bodies[0].ApplyForce(new Vector2(0, -3));
        }

        public void Idle()
        {
            Wheels.ForEach(w => w.MotorEnabled = false);
        }

        public void Decay()
        {
            foreach (var wheel in Wheels)
            {
                if (wheel.MotorSpeed > 0) wheel.MotorSpeed -= 0.4f;
                if (wheel.MotorSpeed < 0) wheel.MotorSpeed += 0.4f;
            }
        }
    }
}
