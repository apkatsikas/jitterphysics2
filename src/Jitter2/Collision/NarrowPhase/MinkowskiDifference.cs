/*
 * Copyright (c) Thorben Linneweber and others
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Runtime.CompilerServices;
using Jitter2.LinearMath;

namespace Jitter2.Collision;

/// <summary>
/// Stores data representing the Minkowski Difference, also known as the Configuration Space Object (CSO),
/// between two support functions: SupportA and SupportB. SupportB is transformed according to the specified
/// OrientationB and PositionB.
/// </summary>
public struct MinkowskiDifference
{
    /// <summary>
    /// Represents a vertex utilized in algorithms that operate on the Minkowski sum of two shapes.
    /// </summary>
    public struct Vertex
    {
        public JVector V;
        public JVector A;
        public JVector B;

        public Vertex(JVector v)
        {
#if NET6_0
            A = new JVector();
            B = new JVector();
#endif
            V = v;
        }
    }

    public ISupportMappable SupportA, SupportB;
    public JQuaternion OrientationB;
    public JVector PositionB;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void SupportMapTransformedRelative(in JVector direction, out JVector result)
    {
        JVector.ConjugatedTransform(direction, OrientationB, out JVector tmp);
        SupportB.SupportMap(tmp, out result);
        JVector.Transform(result, OrientationB, out result);
        JVector.Add(result, PositionB, out result);
    }

    /// <summary>
    /// Calculates the support function S_{A-B}(d) = S_{A}(d) - S_{B}(-d), where "d" represents the direction.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Support(in JVector direction, out Vertex v)
    {
        JVector.Negate(direction, out JVector tmp);
        SupportA.SupportMap(direction, out v.A);
        SupportMapTransformedRelative(tmp, out v.B);
        JVector.Subtract(v.A, v.B, out v.V);
    }

    /// <summary>
    /// Retrieves a point within the Minkowski Difference.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void GetCenter(out Vertex center)
    {
        SupportA.GetCenter(out center.A);
        SupportB.GetCenter(out center.B);
        JVector.Transform(center.B, OrientationB, out center.B);
        JVector.Add(PositionB, center.B, out center.B);
        JVector.Subtract(center.A, center.B, out center.V);
    }
}