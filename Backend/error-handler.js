module.exports = (err, req, res, next) => {
    console.log(err);

    if (err.error !== undefined) {
        console.log(err.error);
    }

    res.statusMessage = err.message;

    res.status(err.code || 500).send();
};