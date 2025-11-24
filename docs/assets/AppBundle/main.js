import { dotnet } from './_framework/dotnet.js'

const runtime = await dotnet
    .withDiagnosticTracing(false)
    .create();

const config = runtime.getConfig();
const exports = await runtime.getAssemblyExports(config.mainAssemblyName);

const canvas = document.getElementById('canvas');
dotnet.instance.Module['canvas'] = canvas;

function mainLoop() {
    exports.RaylibWasm.Application.UpdateFrame(canvas.clientWidth, canvas.clientHeight);

    window.requestAnimationFrame(mainLoop);
}

await runtime.runMain();
window.requestAnimationFrame(mainLoop);

var peer = new Peer();
peer.on('open', function (id) {
    console.log('My peer ID is: ' + id);
});