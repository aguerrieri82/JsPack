const { exec } = require('child_process');
const path = require('path');

const jsPack = '../bin/Debug/net6.0/JsPack.exe'

function getEntry(module) {

    if (module.issuer && module.issuer.resource) 
        return getEntry(module.issuer);

    return module.resource;  
}

module.exports = function () {

    const cmd = path.resolve(__dirname, jsPack) + ' expand "' + getEntry(this._module) + '" "' + this._module.resource + '"';

    var callback = this.async();

    exec(cmd, (_, stdout) => {

        callback(null, stdout);
    });    
}