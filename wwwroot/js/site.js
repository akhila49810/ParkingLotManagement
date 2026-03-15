

(function () {
    'use strict';

    

    function getTagNumber() {
        return document.getElementById('tagNumber').value.trim().toUpperCase();
    }

    function clearTag() {
        document.getElementById('tagNumber').value = '';
        document.getElementById('tagNumber').focus();
    }

    function showAlert(message, type) {
        var box = document.getElementById('alertBox');
        box.textContent = message;
        box.className = 'alert-box alert-' + type;
        box.style.display = 'block';

        clearTimeout(box._timeout);
        box._timeout = setTimeout(function () {
            box.style.display = 'none';
        }, 5000);
    }

    function hideAlert() {
        document.getElementById('alertBox').style.display = 'none';
    }

    function showCheckoutResult(tagNumber, amount) {
        document.getElementById('resultTag').textContent = tagNumber;
        document.getElementById('resultAmount').textContent = '$' + amount.toFixed(2);
        document.getElementById('checkoutResult').style.display = 'block';

        clearTimeout(window._checkoutTimeout);
        window._checkoutTimeout = setTimeout(function () {
            document.getElementById('checkoutResult').style.display = 'none';
        }, 10000);
    }

    function hideCheckoutResult() {
        document.getElementById('checkoutResult').style.display = 'none';
    }

    function setButtonsDisabled(disabled) {
        document.getElementById('btnIn').disabled = disabled;
        document.getElementById('btnOut').disabled = disabled;
    }

    function updateSnapshot(html) {
        var panel = document.getElementById('snapshotPanel');
        panel.innerHTML = html;
        panel.classList.remove('snapshot-refresh');
        void panel.offsetWidth;
        panel.classList.add('snapshot-refresh');
    }

    function postJson(url, body, callback) {
        var xhr = new XMLHttpRequest();
        xhr.open('POST', url, true);
        xhr.setRequestHeader('Content-Type', 'application/json');
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');

        xhr.onload = function () {
            if (xhr.status >= 200 && xhr.status < 300) {
                try {
                    callback(null, JSON.parse(xhr.responseText));
                } catch (e) {
                    callback('Failed to parse server response.');
                }
            } else {
                callback('Server error: ' + xhr.status);
            }
        };

        xhr.onerror = function () {
            callback('Network error. Please try again.');
        };

        xhr.send(JSON.stringify(body));
    }

    function getJson(url, callback) {
        var xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');

        xhr.onload = function () {
            if (xhr.status >= 200 && xhr.status < 300) {
                try {
                    callback(null, JSON.parse(xhr.responseText));
                } catch (e) {
                    callback('Failed to parse server response.');
                }
            } else {
                callback('Server error: ' + xhr.status);
            }
        };

        xhr.onerror = function () {
            callback('Network error. Please try again.');
        };

        xhr.send();
    }

    

    function renderSnapshot(data) {
        var available = data.availableSpots;
        var total = data.totalSpots;
        var fee = data.hourlyFee;
        var cars = data.parkedCars || [];
        var taken = data.spotsTaken;

        var availableClass = available === 0 ? 'text-danger' : 'text-success';

        var tableRows = '';
        if (cars.length === 0) {
            tableRows = '<div class="empty-lot"><div class="empty-icon">🏁</div><p>No cars currently parked</p></div>';
        } else {
            var rows = cars.map(function (car) {
                return '<tr>' +
                    '<td class="tag-cell">' + escapeHtml(car.tagNumber) + '</td>' +
                    '<td>' + escapeHtml(car.checkInTime) + '</td>' +
                    '<td>' + escapeHtml(car.elapsedTime) + '</td>' +
                    '</tr>';
            }).join('');

            tableRows = '<table class="table parking-table">' +
                '<thead><tr>' +
                '<th>Tag Number</th><th>Time In</th><th>Elapsed Time</th>' +
                '</tr></thead>' +
                '<tbody>' + rows + '</tbody>' +
                '</table>';
        }

        var html =
            '<div class="snapshot-header">' +
            '<div class="snapshot-info">' +
            '<span class="info-badge">Total Spots: <strong>' + total + '</strong></span>' +
            '<span class="info-badge">Hourly Fee: <strong>$' + fee.toFixed(2) + '</strong></span>' +
            '</div>' +
            '<div class="available-count">Available Spots: <span class="available-number ' + availableClass + '">' + available + '</span></div>' +
            '</div>' +
            tableRows +
            '<div class="snapshot-footer">Spots taken: <strong>' + taken + '</strong></div>';

        updateSnapshot(html);
    }

    function escapeHtml(str) {
        var d = document.createElement('div');
        d.appendChild(document.createTextNode(str));
        return d.innerHTML;
    }

   

    window.handleCheckIn = function () {
        var tag = getTagNumber();
        if (!tag) {
            showAlert('Please enter a tag number.', 'error');
            return;
        }

        hideAlert();
        hideCheckoutResult();
        setButtonsDisabled(true);

        postJson('/Home/CheckIn', { tagNumber: tag }, function (err, data) {
            setButtonsDisabled(false);

            if (err) {
                showAlert(err, 'error');
                return;
            }

            if (!data.success) {
                showAlert(data.errorMessage, 'error');
            } else {
                showAlert('Car ' + tag + ' checked in successfully!', 'success');
                clearTag();
                renderSnapshot(data.snapshot);
            }
        });
    };

   

    window.handleCheckOut = function () {
        var tag = getTagNumber();
        if (!tag) {
            showAlert('Please enter a tag number.', 'error');
            return;
        }

        hideAlert();
        hideCheckoutResult();
        setButtonsDisabled(true);

        postJson('/Home/CheckOut', { tagNumber: tag }, function (err, data) {
            setButtonsDisabled(false);

            if (err) {
                showAlert(err, 'error');
                return;
            }

            if (!data.success) {
                showAlert(data.errorMessage, 'error');
            } else {
                clearTag();
                showCheckoutResult(data.tagNumber, data.amountCharged);
                renderSnapshot(data.snapshot);
            }
        });
    };

    

    window.openStats = function () {
        var modal = new bootstrap.Modal(document.getElementById('statsModal'));

        document.getElementById('statsContent').innerHTML =
            '<div class="text-center py-3">' +
            '<div class="spinner-border text-primary" role="status"></div>' +
            '<p class="mt-2 text-muted">Loading stats...</p>' +
            '</div>';

        modal.show();

        getJson('/Home/Stats', function (err, data) {
            if (err) {
                document.getElementById('statsContent').innerHTML =
                    '<div class="alert alert-danger">Failed to load stats.</div>';
                return;
            }

            document.getElementById('statsContent').innerHTML =
                '<div class="stat-grid">' +
                '<div class="stat-card">' +
                '<div class="stat-value text-primary">' + data.availableSpots + '</div>' +
                '<div class="stat-label">Available Spots</div>' +
                '</div>' +
                '<div class="stat-card">' +
                '<div class="stat-value text-success">$' + data.todaysRevenue.toFixed(2) + '</div>' +
                '<div class="stat-label">Today\'s Revenue</div>' +
                '</div>' +
                '<div class="stat-card">' +
                '<div class="stat-value">' + data.averageCarsPerDay.toFixed(1) + '</div>' +
                '<div class="stat-label">Avg Cars / Day (30d)</div>' +
                '</div>' +
                '<div class="stat-card">' +
                '<div class="stat-value text-warning">$' + data.averageRevenuePerDay.toFixed(2) + '</div>' +
                '<div class="stat-label">Avg Revenue / Day (30d)</div>' +
                '</div>' +
                '</div>';
        });
    };

    

    document.addEventListener('DOMContentLoaded', function () {
        var input = document.getElementById('tagNumber');
        if (input) {
            input.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    handleCheckIn();
                }
            });
        }
    });

})();
