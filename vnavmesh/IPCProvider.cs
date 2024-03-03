﻿using Navmesh.Movement;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Navmesh
{
    class IPCProvider : IDisposable
    {
        private List<Action> _disposeActions = new();

        public IPCProvider(NavmeshManager navmeshManager, FollowPath followPath, AsyncMoveRequest move, MainWindow mainWindow)
        {
            Register("Nav.IsReady", () => navmeshManager.Navmesh != null);
            Register("Nav.BuildProgress", () => navmeshManager.LoadTaskProgress);
            Register("Nav.Reload", () => navmeshManager.Reload(true));
            Register("Nav.Rebuild", () => navmeshManager.Reload(false));
            Register("Nav.Pathfind", (Vector3 from, Vector3 to, bool fly) => navmeshManager.QueryPath(from, to, fly));
            Register("Nav.IsAutoLoad", () => navmeshManager.AutoLoad);
            Register("Nav.SetAutoLoad", (bool v) => navmeshManager.AutoLoad = v);

            Register("Query.Mesh.NearestPoint", (Vector3 p, float halfExtentXZ, float halfExtentY) => navmeshManager.Query?.FindNearestPointOnMesh(p, halfExtentXZ, halfExtentY));
            Register("Query.Mesh.PointOnFloor", (Vector3 p, float halfExtentXZ) => navmeshManager.Query?.FindPointOnFloor(p, halfExtentXZ));

            Register("Path.MoveTo", (List<Vector3> waypoints, bool fly) => followPath.Move(waypoints, !fly));
            Register("Path.Stop", followPath.Stop);
            Register("Path.IsRunning", () => followPath.Waypoints.Count > 0);
            Register("Path.NumWaypoints", () => followPath.Waypoints.Count);
            Register("Path.GetMovementAllowed", () => followPath.MovementAllowed);
            Register("Path.SetMovementAllowed", (bool v) => followPath.MovementAllowed = v);
            Register("Path.GetAlignCamera", () => followPath.AlignCamera);
            Register("Path.SetAlignCamera", (bool v) => followPath.AlignCamera = v);
            Register("Path.GetTolerance", () => followPath.Tolerance);
            Register("Path.SetTolerance", (float v) => followPath.Tolerance = v);

            Register("SimpleMove.PathfindAndMoveTo", (Vector3 dest, bool fly) => move.MoveTo(dest, fly));
            Register("SimpleMove.PathfindInProgress", () => move.TaskInProgress);

            Register("Window.IsOpen", () => mainWindow.IsOpen);
            Register("Window.SetOpen", (bool v) => mainWindow.IsOpen = v);
        }

        public void Dispose()
        {
            foreach (var a in _disposeActions)
                a();
        }

        private void Register<TRet>(string name, Func<TRet> func)
        {
            var p = Service.PluginInterface.GetIpcProvider<TRet>("vnavmesh." + name);
            p.RegisterFunc(func);
            _disposeActions.Add(p.UnregisterFunc);
        }

        private void Register<TRet, T1>(string name, Func<T1, TRet> func)
        {
            var p = Service.PluginInterface.GetIpcProvider<T1, TRet>("vnavmesh." + name);
            p.RegisterFunc(func);
            _disposeActions.Add(p.UnregisterFunc);
        }

        private void Register<TRet, T1, T2>(string name, Func<T1, T2, TRet> func)
        {
            var p = Service.PluginInterface.GetIpcProvider<T1, T2, TRet>("vnavmesh." + name);
            p.RegisterFunc(func);
            _disposeActions.Add(p.UnregisterFunc);
        }

        private void Register<TRet, T1, T2, T3>(string name, Func<T1, T2, T3, TRet> func)
        {
            var p = Service.PluginInterface.GetIpcProvider<T1, T2, T3, TRet>("vnavmesh." + name);
            p.RegisterFunc(func);
            _disposeActions.Add(p.UnregisterFunc);
        }

        private void Register(string name, Action func)
        {
            var p = Service.PluginInterface.GetIpcProvider<object>("vnavmesh." + name);
            p.RegisterAction(func);
            _disposeActions.Add(p.UnregisterAction);
        }

        private void Register<T1>(string name, Action<T1> func)
        {
            var p = Service.PluginInterface.GetIpcProvider<T1, object>("vnavmesh." + name);
            p.RegisterAction(func);
            _disposeActions.Add(p.UnregisterAction);
        }

        private void Register<T1, T2>(string name, Action<T1, T2> func)
        {
            var p = Service.PluginInterface.GetIpcProvider<T1, T2, object>("vnavmesh." + name);
            p.RegisterAction(func);
            _disposeActions.Add(p.UnregisterAction);
        }
    }
}
