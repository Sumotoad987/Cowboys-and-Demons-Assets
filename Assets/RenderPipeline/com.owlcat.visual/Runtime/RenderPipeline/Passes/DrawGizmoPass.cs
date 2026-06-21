using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes
{
    public class DrawGizmoPass : ScriptableRenderPass
    {
        private string m_Name;
        private ProfilingSampler m_ProfilingSampler;
        private GizmoSubset m_GizmoSubset;

        public override string Name => "Draw Gizmo";

        public DrawGizmoPass(GizmoSubset gizmoSubset)
        {
            this.RenderPassEvent = gizmoSubset == GizmoSubset.PreImageEffects ? RenderPassEvent.AfterRenderingTransparents : (RenderPassEvent) 1001;
            this.m_Name = string.Format("{0}.{1}", (object)"DrawGizmosPass", (object)this.m_GizmoSubset);
            this.m_ProfilingSampler = new ProfilingSampler(this.m_Name);
            this.m_GizmoSubset = gizmoSubset;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            CommandBuffer cmd = CommandBufferPool.Get("Draw Gizmo");
            using (new ProfilingScope(cmd, this.m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                context.DrawGizmos(renderingData.CameraData.Camera, this.m_GizmoSubset);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
