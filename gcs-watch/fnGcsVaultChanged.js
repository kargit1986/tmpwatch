exports.fnGcsVaultChanged = (event,callback) => {
    const file = event.data;
    console.log(`  Event: ${event.eventId}`);
    console.log(`  Event Type: ${event.eventType}`);
    console.log(`  Bucket: ${file.bucket}`);
    console.log(`  File: ${file.name}`);
    console.log(`  Metageneration: ${file.metageneration}`);
    console.log(`  Created: ${file.timeCreated}`);
    console.log(`  Updated: ${file.updated}`);
    callback();
}