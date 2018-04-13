/*
CryptoJS v3.1
code.google.com/p/crypto-js
(c) 2009-2013 by Jeff Mott. All rights reserved.
code.google.com/p/crypto-js/wiki/License
*/
(function () {
    // Check if typed arrays are supported
    if (typeof ArrayBuffer != 'function') {
        return;
    }

    // Shortcuts
    var C = CryptoJS;
    var C_lib = C.lib;
    var WordArray = C_lib.WordArray;

    // Reference original init
    var $superInit = WordArray.init;

    // Augment WordArray.init to handle typed arrays
    WordArray.init = function (typedArray) {
        // Convert buffers to data view
        if (typedArray instanceof ArrayBuffer) {
            typedArray = new DataView(typedArray);
        }

        // Convert array views to data view
        if (
            typedArray instanceof Int8Array ||
            typedArray instanceof Uint8Array ||
            typedArray instanceof Uint8ClampedArray ||
            typedArray instanceof Int16Array ||
            typedArray instanceof Uint16Array ||
            typedArray instanceof Int32Array ||
            typedArray instanceof Uint32Array ||
            typedArray instanceof Float32Array ||
            typedArray instanceof Float64Array
        ) {
            typedArray = new DataView(typedArray.buffer);
        }

        // Handle data views
        if (typedArray instanceof DataView) {
            var typedArrayByteLength = typedArray.byteLength;

            var words = [];
            for (var i = 0; i < typedArrayByteLength; i++) {
                words[i >>> 2] |= typedArray.getUint8(i) << (24 - (i % 4) * 8);
            }

            $superInit.call(this, words, typedArrayByteLength);
        } else {
            // Else call normal init
            $superInit.apply(this, arguments);
        }
    };

    WordArray.init.prototype = WordArray;
}());
