// Triggas när C#-servern svarat på handshake
function msg_int(val) {
    var device = new LiveAPI("live_set view selected_device");
    if (!device) return;

    var count = device.getcount("parameters");
    var output = ["/ia/device/parameters"];

    for (var i = 0; i < count; i++) {
        var param = new LiveAPI("live_set view selected_device parameters " + i);
        // Vi skickar ID och det SYNLIGA namnet (t.ex. "Cutoff")
        output.push(i);
        output.push(param.get("name").toString());
    }

    // Skicka hela listan till C#-servern i ett svep
    outlet(0, output);
}