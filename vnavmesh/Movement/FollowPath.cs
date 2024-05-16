﻿using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Navmesh.Movement;

public class FollowPath : IDisposable
{
    public bool MovementAllowed = true;
    public bool IgnoreDeltaY = false;
    public float Tolerance = 0.25f;
    public List<Vector3> Waypoints = new();

    private NavmeshManager _manager;
    private OverrideCamera _camera = new();
    private OverrideMovement _movement = new();
    private DateTime _nextJump;

    public FollowPath(NavmeshManager manager)
    {
        _manager = manager;
        _manager.OnNavmeshChanged += OnNavmeshChanged;
        OnNavmeshChanged(_manager.Navmesh, _manager.Query);
    }

    public void Dispose()
    {
        _manager.OnNavmeshChanged -= OnNavmeshChanged;
        _camera.Dispose();
        _movement.Dispose();
    }

    public unsafe void Update()
    {
        var player = Service.ClientState.LocalPlayer;
        if (player == null)
            return;

        while (Waypoints.Count > 0)
        {
            var toNext = Waypoints[0] - player.Position;
            if (IgnoreDeltaY)
                toNext.Y = 0;
            if (toNext.LengthSquared() > Tolerance * Tolerance)
                break;
            Waypoints.RemoveAt(0);
        }

        if (Waypoints.Count == 0)
        {
            _movement.Enabled = _camera.Enabled = false;
            _camera.SpeedH = _camera.SpeedV = default;
            _movement.DesiredPosition = player.Position;
        }
        else
        {
            OverrideAFK.ResetTimers();
            _movement.Enabled = MovementAllowed;
            _movement.DesiredPosition = Waypoints[0];
            if (_movement.DesiredPosition.Y > player.Position.Y && !Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InFlight] && !IgnoreDeltaY) //Only do this bit if on a flying path
            {
                // walk->fly transition (TODO: reconsider?)
                if (Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.Mounted])
                    ExecuteJump(); // Spam jump to take off
                else
                {
                    _movement.Enabled = false; // Don't move, since it'll just run on the spot
                    return;
                }
            }

            _camera.Enabled = Service.Config.AlignCameraToMovement;
            _camera.SpeedH = _camera.SpeedV = 360.Degrees();
            _camera.DesiredAzimuth = Angle.FromDirectionXZ(_movement.DesiredPosition - player.Position) + 180.Degrees();
            _camera.DesiredAltitude = -30.Degrees();
        }
    }

    public void Stop() => Waypoints.Clear();

    private unsafe void ExecuteJump()
    {
        if (DateTime.Now >= _nextJump)
        {
            ActionManager.Instance()->UseAction(ActionType.GeneralAction, 2);
            _nextJump = DateTime.Now.AddMilliseconds(100);
        }
    }

    public void Move(List<Vector3> waypoints, bool ignoreDeltaY)
    {
        Waypoints = waypoints;
        IgnoreDeltaY = ignoreDeltaY;
    }

    private void OnNavmeshChanged(Navmesh? navmesh, NavmeshQuery? query)
    {
        Waypoints.Clear();
    }
}
