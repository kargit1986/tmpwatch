const TOKEN = "3GcYVcG2EHRSgjn7zXBQ0Fzt";
const HOST = "http://35.188.185.218";
var options = {
    apiVersion: "v1",
    endPoint: HOST,
    token: TOKEN
};
var vaultModule = require("node-vault");
var vault = vaultModule(options);
var k = vault.read('secret/hello').then((data) => console.log(data));
//vault.write('secret/hello', { value: 'world', lease: '1s' })
//.then( () => vault.read('secret/hello')).catch((error) => console.log(error));

metadataCached = {};
isFirst = true;

// var processSecrets = async function processSecrets() {
//     var metadataResult = await vault.read('secret/metadata?list=true');
//     var keys = metadataResult.data.keys;
//     var length = keys.length;
//     for (var i = 0; i < length; i++) {
//         try {
//             var key = keys[i];
//             var keyMetadata = await vault.read(`secret/metadata/${key}`);
//             var versions = keyMetadata.data.versions;
//             var isNewMetadata = false;
//             if (metadataCached[key] == undefined) {
//                 metadataCached[key] = {};
//                 isNewMetadata = true;
//             }
//             for (var versionNum in versions) {
//                 var version = versions[versionNum];
//                 var isDeleted = !(version.deletion_time == "");
//                 if (metadataCached[key][versionNum] == undefined) {
//                     metadataCached[key][versionNum] = isDeleted;
//                     isNewMetadata = true;
//                 }
//                 else if (metadataCached[key][versionNum] != isDeleted) {
//                     metadataCached[key][versionNum] = isDeleted;
//                     isNewMetadata = true;
//                 }
//                 if(isNewMetadata && !isFirst)
//                 {
//                     var newSecret = {
//                         key : key,
//                         version : versionNum,
//                         isDeleted : isDeleted
//                     };
//                     //newSecrets.push(newSecret);
//                     console.log(`new secret with properties ${newSecret.key}  ${newSecret.isDeleted} ${newSecret.version}`);
//                 }
//             }
//             if (i == length - 1) {
//                 if(isFirst)
//                 {
//                     isFirst = false;
//                 }
//                 console.log('loop finished ');
//             }
//         }
//         catch (exc) {
//             console.log(exc);
//         }
//     }
// }

// processSecrets();


// exports.fnGcsVaultChanged = (event,callback) => {
//     const file = event.data;
//     console.log(` Fired on  Bucket: ${file.bucket}`);
//     callback();
// }
