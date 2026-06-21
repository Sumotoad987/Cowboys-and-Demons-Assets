using System;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ProfilingScope : IDisposable
{
	public ProfilingScope(CommandBuffer cmd, ProfilingSampler sampler)
	{
	}

	public void Dispose()
	{
	}
}
