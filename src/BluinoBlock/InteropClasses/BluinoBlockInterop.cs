using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BluinoBlock
{
    public static class BluinoBlockInterop
    {
        // Blockly
        internal static ValueTask<object> DemoWorkspace(
            IJSRuntime jsRuntime,
            ElementReference blocklyDiv,
            ElementReference toolbox,
            ElementReference startBlocks)
        {
            return jsRuntime.InvokeAsync<object>(
                "BlocklyFunctions.createDemoWorkspace",
                blocklyDiv,
                toolbox,
                startBlocks);
        }

        internal static ValueTask<string> GetXML(
            IJSRuntime jsRuntime)
        {
            return jsRuntime.InvokeAsync<string>(
                "BlocklyFunctions.getXML");
        }
    }
}