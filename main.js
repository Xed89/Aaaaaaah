import { dotnet } from './_framework/dotnet.js'

const runtime = await dotnet
    .withDiagnosticTracing(false)
    .create();

const config = runtime.getConfig();
const exports = await runtime.getAssemblyExports(config.mainAssemblyName);

var peer;
var peerJsConnections = [];

function setupConnection(conn) {
    var connectionId = peerJsConnections.push(conn);
    conn.on('data', function (data) {
        console.log('received data');
        exports.RaylibWasm.PeerJsInterop.OnData(connectionId, new Uint8Array(data));
    });
    exports.RaylibWasm.PeerJsInterop.OnConnect(connectionId);
}

await runtime.setModuleImports("PeerJsInterop",
    {
        "CreatePeerJs": function (id) {
            console.log("Creating PeerJs with id=" + id);
            peer = new Peer(id);
            peer.on('open', function (id) {
                console.log('My peer ID is: ' + id);
                exports.RaylibWasm.PeerJsInterop.OnOpen();
            });
            peer.on('error', function (err) {
                console.log('connection error: ' + err.type);
            });
            peer.on('disconnected', function () {
                console.log('disconnected');
            });
            peer.on('connection', function (conn) {
                setupConnection(conn);
            });
            return;
        },

        "ConnectTo": function (id) {
            console.log("Connecting to " + id);
            var conn = peer.connect(id);
            setupConnection(conn);
        },

        "SendData": function (connectionId, data) {
            console.log("Sending data conn #" + connectionId);
            peerJsConnections[connectionId - 1].send(data);
        },

        "Prompt": function (message, defaultValue) {
            return prompt(message, defaultValue);
        }
    }
)

const canvas = document.getElementById('canvas');
dotnet.instance.Module['canvas'] = canvas;

function mainLoop() {
    exports.RaylibWasm.Application.UpdateFrame(canvas.clientWidth, canvas.clientHeight);

    window.requestAnimationFrame(mainLoop);
}

await runtime.runMain();
window.requestAnimationFrame(mainLoop);

// var peer = new Peer();
// peer.on('open', function (id) {
//     console.log('My peer ID is: ' + id);
// });