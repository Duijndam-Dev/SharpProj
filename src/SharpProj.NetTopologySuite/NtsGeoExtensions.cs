﻿using System;
using SharpProj;
using SharpProj.NTS;

using DistanceOp = NetTopologySuite.Operation.Distance.DistanceOp;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// 
    /// </summary>
    public static class SharpProjNtsExtensions
    {
        /// <summary>
        /// Returns the minimum distance between this Geometry <paramref name="g0"/> and another Geometry <paramref name="g1"/>, by calculating the nearest
        /// points in NTS and then asking (Sharp)Proj to get the actual distance in meters.
        /// </summary>
        /// <param name="g0">Geometry 1</param>
        /// <param name="g1">Geometry 2</param>
        /// <returns>The distance in meters between g0 and g1, or null if unable to calculate</returns>
        /// <exception cref="ArgumentNullException">g0 or g1 is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">The SRID of g0 and g1 don't match, are 0 or can't be resolved using <see cref="SridRegister"/></exception>
        /// <exception cref="ArgumentException"></exception>
        public static double? MeterDistance(this Geometry g0, Geometry g1)
        {
            if (g0 == null)
                throw new ArgumentNullException(nameof(g0));
            else if (g1 == null)
                throw new ArgumentNullException(nameof(g1));

            int srid = g0.SRID;
            if (srid == 0 || g1.SRID != g0.SRID)
                throw new ArgumentOutOfRangeException("SRID is 0 or doesn't match");

            SridItem sridItem;
            try
            {
                sridItem = SridRegister.GetByValue(srid);
            }
            catch (IndexOutOfRangeException sridExcepton)
            {
                throw new ArgumentOutOfRangeException("SRID not resolveable", sridExcepton);
            }

            using (var dt = sridItem.CRS.DistanceTransform.Clone()) // Thread safe with clone
            {
                return g0.MeterDistance(g1, dt);
            }
        }

        private static double? MeterDistance(this Geometry g0, Geometry g1, CoordinateTransform dt)
        {
            DistanceOp distanceOp = new DistanceOp(g0, g1);
            Coordinate[] nearestPoints = distanceOp.NearestPoints();

            double d = dt.GeoDistance(nearestPoints[0].ToPPoint(), nearestPoints[1].ToPPoint());

            if (double.IsInfinity(d) || double.IsNaN(d))
                return null;
            else
                return d;
        }

        /// <summary>
        /// Returns a boolean indicating whether this Geometry <paramref name="g0"/> is within <paramref name="distanceInMeter"/> meters
        /// of Geometry <paramref name="g1"/>.
        /// </summary>
        /// <param name="g0">Geometry 1</param>
        /// <param name="g1">Geometry 2</param>
        /// <param name="distanceInMeter">The distance limit</param>
        /// <returns>true if the geometries are within distance, false if not and NULL if unable to calculate</returns>
        /// <exception cref="ArgumentNullException">g0 or g1 is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">The SRID of g0 and g1 don't match, are 0 or can't be resolved using <see cref="SridRegister"/></exception>
        /// <exception cref="ArgumentException"></exception>
        public static bool? IsWithinMeterDistance(this Geometry g0, Geometry g1, double distanceInMeter)
        {
            if (g0 == null)
                throw new ArgumentNullException(nameof(g0));
            else if (g1 == null)
                throw new ArgumentNullException(nameof(g1));

            int srid = g0.SRID;
            if (srid == 0 || g1.SRID != g0.SRID)
                throw new ArgumentOutOfRangeException("SRID is 0 or doesn't match");

            DistanceOp distanceOp = new DistanceOp(g0, g1);
            Coordinate[] nearestPoints = distanceOp.NearestPoints();

            SridItem sridItem;
            try
            {
                sridItem = SridRegister.GetByValue(srid);
            }
            catch (IndexOutOfRangeException sridExcepton)
            {
                throw new ArgumentOutOfRangeException("SRID not resolveable", sridExcepton);
            }

            // TODO: There should be possible optimizations now, with knowledg of the CRS
            //       Maybe we can assume that this is always false if the NTS cut-off thinks it's false
            //       Not 100% sure and we want correctness

            var dt = sridItem.CRS.DistanceTransform; // Distance calculations are thread safe.

            double d = dt.GeoDistance(nearestPoints[0].ToPPoint(), nearestPoints[1].ToPPoint());

            if (double.IsInfinity(d) || double.IsNaN(d))
                return null;
            else
                return (d <= distanceInMeter);
        }

        /// <summary>
        /// When the Coordinates are <see cref="CoordinateZ"/> uses <see cref="CoordinateZ.Equals3D(CoordinateZ)"/>, otherwise do a 2D check and verify that coordinates are 2D
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool Equals3D(this Coordinate coordinate, Coordinate other)
        {
            if (coordinate is null || other is null)
                return (coordinate is null == other is null);

            if (coordinate is CoordinateZ z1 && other is CoordinateZ z2)
                return z1.Equals3D(z2);
            else
                return coordinate.Equals2D(other)
                    && double.IsNaN(coordinate.Z) == double.IsNaN(other.Z);
        }

        /// <summary>
        /// When the Coordinates are <see cref="CoordinateZ"/> uses <see cref="CoordinateZ.Equals3D(CoordinateZ)"/>, otherwise do a 2D check and verify that coordinates are 2D
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool Equals3D(this Coordinate coordinate, Coordinate other, double tolerance)
        {
            if (coordinate is null || other is null)
                return (coordinate is null == other is null);

            if (coordinate is CoordinateZ z1 && other is CoordinateZ z2)
                return z1.Equals3D(z2, tolerance);
            else
                return coordinate.Equals2D(other)
                    && double.IsNaN(coordinate.Z) == double.IsNaN(other.Z);
        }

        /// <summary>
        /// Returns the total length between the coordinates of <paramref name="l"/> in meters via SharpProk
        /// </summary>
        /// <param name="l">The linestring</param>
        /// <returns>The distance in meters between the coordinates, or null if unable to calculate</returns>
        /// <exception cref="ArgumentException"></exception>
        public static double? MeterLength(this LineString l)
        {
            if (l == null)
                throw new ArgumentNullException(nameof(l));

            int srid = l.SRID;
            if (srid == 0)
                throw new ArgumentOutOfRangeException("SRID is 0 or doesn't match");


            SridItem sridItem;
            try
            {
                sridItem = SridRegister.GetByValue(srid);
            }
            catch (IndexOutOfRangeException sridExcepton)
            {
                throw new ArgumentOutOfRangeException("SRID not resolveable", sridExcepton);
            }

            using (var dt = sridItem.CRS.DistanceTransform.Clone()) // Thread safe with clone
            {
                return l.MeterLength(dt);
            }
        }

        private static double? MeterLength(this LineString l, CoordinateTransform dt)
        {
            double d = dt.GeoDistance(l.CoordinateSequence.ToPPoints());

            if (double.IsInfinity(d) || double.IsNaN(d))
                return null;
            else
                return d;
        }

        /// <summary>
        /// Returns the peremiter of this polygon in meters (via ProjSharp)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double? MeterLength(this Polygon p)
        {
            if (p == null)
                throw new ArgumentNullException(nameof(p));

            int srid = p.SRID;
            if (srid == 0)
                throw new ArgumentOutOfRangeException("SRID is 0 or doesn't match");


            SridItem sridItem;
            try
            {
                sridItem = SridRegister.GetByValue(srid);
            }
            catch (IndexOutOfRangeException sridExcepton)
            {
                throw new ArgumentOutOfRangeException("SRID not resolveable", sridExcepton);
            }

            using (var dt = sridItem.CRS.DistanceTransform.Clone()) // Thread safe with clone
            {
                return p.MeterLength(dt);
            }
        }

        private static double? MeterLength(this Polygon p, CoordinateTransform dt)
        {
            double? len = p.ExteriorRing.MeterLength(dt);

            for (int i = 0; i < p.NumInteriorRings; i++)
            {
                len += p.GetInteriorRingN(i).MeterLength(dt);
            }

            return len;
        }


        /// <summary>
        /// Gets the area occupied by polygon <paramref name="p"/> in square meters (via SharpProj)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double? MeterArea(this Polygon p)
        {
            if (p == null)
                throw new ArgumentNullException(nameof(p));

            int srid = p.SRID;
            if (srid == 0)
                throw new ArgumentOutOfRangeException("SRID is 0 or doesn't match");


            SridItem sridItem;
            try
            {
                sridItem = SridRegister.GetByValue(srid);
            }
            catch (IndexOutOfRangeException sridExcepton)
            {
                throw new ArgumentOutOfRangeException("SRID not resolveable", sridExcepton);
            }


            using (var dt = sridItem.CRS.DistanceTransform.Clone())
            {
                return MeterArea(p, dt);
            }
        }

        private static double? MeterArea(this Polygon p, CoordinateTransform dt)
        {
            double? s = SignedRingArea(p.ExteriorRing, dt);

            if (!s.HasValue)
                return null;

            foreach (LineString ls in p.InteriorRings)
            {
                double? s2 = SignedRingArea(ls, dt);

                if (!s2.HasValue)
                    return null;

                s += s2;
            }

            return Math.Abs(s.Value);
        }

        private static double? SignedRingArea(LineString ring, CoordinateTransform dt)
        {
            double d = dt.GeoArea(ring.CoordinateSequence.ToPPoints());

            if (double.IsInfinity(d) || double.IsNaN(d))
                return null;
            else
                return d;
        }

        /// <summary>
        /// Gets the meterlengths of the <see cref="Polygon"/>, <see cref="LineString"/> and <see cref="GeometryCollection"/> instances in <paramref name="gc"/> in meters
        /// </summary>
        /// <param name="gc"></param>
        /// <returns></returns>
        public static double? MeterLength(this GeometryCollection gc)
        {
            if (gc.Count == 0)
                return null;

            int srid = gc.SRID;
            if (srid == 0)
                throw new ArgumentOutOfRangeException("SRID is 0 or doesn't match");

            SridItem sridItem;
            try
            {
                sridItem = SridRegister.GetByValue(srid);
            }
            catch (IndexOutOfRangeException sridExcepton)
            {
                throw new ArgumentOutOfRangeException("SRID not resolveable", sridExcepton);
            }

            using (var dt = sridItem.CRS.DistanceTransform.Clone()) // Thread safe with clone
            {
                return gc.MeterLength(dt);
            }
        }

        private static double? MeterLength(this GeometryCollection gc, CoordinateTransform dt)
        {
            double sum = 0;

            foreach (Geometry g in gc)
            {
                double? s = (g as LineString)?.MeterLength(dt)
                        ?? (g as Polygon).MeterLength(dt)
                        ?? (g as GeometryCollection)?.MeterLength(dt);

                if (!s.HasValue)
                    return null;

                sum += s.Value;
            }
            return sum;
        }

        /// <summary>
        /// Gets the area occupied by the polygons in <paramref name="gc"/> in square meters
        /// </summary>
        /// <param name="gc"></param>
        /// <returns></returns>
        public static double? MeterArea(this GeometryCollection gc)
        {
            if (gc.Count == 0)
                return null;

            int srid = gc.SRID;
            if (srid == 0)
                throw new ArgumentOutOfRangeException("SRID is 0 or doesn't match");

            SridItem sridItem;
            try
            {
                sridItem = SridRegister.GetByValue(srid);
            }
            catch (IndexOutOfRangeException sridExcepton)
            {
                throw new ArgumentOutOfRangeException("SRID not resolveable", sridExcepton);
            }

            using (var dt = sridItem.CRS.DistanceTransform.Clone()) // Thread safe with clone
            {
                return gc.MeterArea(dt);
            }
        }

        private static double? MeterArea(this GeometryCollection gc, CoordinateTransform dt)
        {
            double sum = 0.0;

            // TODO: Optimize by fetching SRID only once
            foreach (Geometry g in gc)
            {
                double? s = (g as Polygon)?.MeterArea(dt)
                    ?? (g as GeometryCollection)?.MeterArea(dt);

                if (!s.HasValue)
                    return null;

                sum += s.Value;
            }
            return sum;
        }
    }
}
