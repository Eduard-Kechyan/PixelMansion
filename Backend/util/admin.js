var admin = require("firebase-admin");

var serviceAccount = require("Backend/util/pixelmergegame-firebase-adminsdk-5vmuc-fbdea7da5e.json");

admin.initializeApp({
    credential: admin.credential.cert(serviceAccount)
});

const db = admin.firestore();
module.exports = { admin, db };