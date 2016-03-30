﻿using UnityEngine;
using System.Collections;
using Generic.Extensions;
using Generic.Framework;

//------------------------------------------------------------------------
//
//  Name:   MovingEntity.h
//
//  Desc:   A base class defining an entity that moves. The entity has 
//          a local coordinate system and members for defining its
//          mass and velocity.
//
//  Author: Mat Buckland 2003 (fup@ai-junkie.com)
//
//------------------------------------------------------------------------

namespace Green
{
    class MovingEntity : Base2DEntity
    {

        Vector2 _velocity;

        //a normalized vector pointing in the direction the entity is heading. 
        Vector2 _heading;

        //a vector perpendicular to the heading vector
        Vector2 _side;

        
        float _mass;

        //the maximum speed this entity may travel at.
        float _maxSpeed;

        //the maximum force this entity can produce to power itself 
        //(think rockets and thrust)
        float _maxForce;

        //the maximum rate (radians per second)this vehicle can rotate         
        float _maxTurnRate;

        GameWorld _world;

        // Vector2 _position;

        public void Init(Vector2 position,
                        float radius,
                        Vector2 velocity,
                        float max_speed,
                        Vector2 heading,
                        float mass,
                        Vector2 scale,
                        float turn_rate,
                        float max_force)
        {
            Position = position;
            _scale = scale;
            _boundingRadius = radius;    
            _heading = heading;
            _velocity = velocity;
            _side = heading.Perpendicular();
            _mass = mass;
            _maxSpeed = max_speed;
            _maxTurnRate = turn_rate;
            _maxForce = max_force;
        }

        //accessors
        public GameWorld World
        {
            get
            {
                return _world;
            }
            set
            {
                _world = value;
            }
        }
        public Vector2 Velocity
        {
            get
            {
                return _velocity;
            }
            set
            {
                _velocity = value;
            }
        }

        public float Mass
        {
            get
            {
                return _mass;
            }
        }

        public Vector2 Side
        {
            get
            {
                return _side;
            }
        }

        public float MaxSpeed
        {
            get
            {
                return _maxSpeed;
            }
            set
            {
                _maxSpeed = value;
            }
        }

        public float MaxForce
        {
            get
            {
                return _maxForce;
            }
            set
            {
                _maxForce = value;
            }
        }


        public bool IsSpeedMaxedOut()
        {
            return _maxSpeed * _maxSpeed >= _velocity.SqrMagnitude();
        }

        public float Speed
        {
            get
            {
                return _velocity.SqrMagnitude();
            }
        }
        public float SpeedSq
        {
            get
            {
                return _velocity.SqrMagnitude();
            }
        }

        public Vector2 Heading
        {
            get
            {
                return _heading;
            }
            //------------------------- SetHeading ----------------------------------------
            //
            //  first checks that the given heading is not a vector of zero length. If the
            //  new heading is valid this fumction sets the entity's heading and side 
            //  vectors accordingly
            //-----------------------------------------------------------------------------
            set
            {
                if(!(value.SqrMagnitude() < float.Epsilon))
                {
                    Debug.LogErrorFormat("heading is a vector of zero length");
                }
                

                _heading = value;
                //the side vector must always be perpendicular to the heading
                _side = _heading.Perpendicular();
            }
        }


        public bool RotateHeadingToFacePosition(Vector2 target)
        {
            Vector2 toTarget = (target - Position).normalized;

            //first determine the angle between the heading vector and the target
            float angle = Mathf.Acos(Vector2.Dot(_heading, toTarget));

            //return true if the player is facing the target
            if (angle < float.Epsilon) return true;

            //clamp the amount to turn to the max turn rate
            if (angle > _maxTurnRate) angle = _maxTurnRate;

            //notice how the direction of rotation has to be determined when creating
            //the rotation matrix
            Quaternion rotate = Quaternion.AngleAxis(angle * _heading.Sign(toTarget), Vector3.forward);

            _heading = rotate * _heading;
            _velocity = rotate * _velocity;

            //finally recreate m_vSide
            _side = _heading.Perpendicular();

            return false;
        }

        public float MaxTurnRate
        {
            get
            {
                return _maxTurnRate;
            }
            set
            {
                _maxTurnRate = value;
            }
        }
    }
}