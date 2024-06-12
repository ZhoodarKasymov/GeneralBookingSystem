// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function getCookie(name) {
    var cookies = document.cookie.split("; ");
    for (var i = 0; i < cookies.length; i++) {
        var cookie = cookies[i].split("=");
        if (cookie[0] === name) {
            return cookie[1];
        }
    }
    return "";
}

function dangerAlert(message) {
    Swal.fire({
        html: `<div class="text-start">
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none">
                      <path d="M12 22C13.3135 22.0017 14.6143 21.7438 15.8278 21.2412C17.0413 20.7385 18.1435 20.001 19.071 19.071C20.001 18.1435 20.7385 17.0413 21.2412 15.8278C21.7438 14.6143 22.0017 13.3135 22 12C22.0017 10.6865 21.7438 9.3857 21.2411 8.17222C20.7385 6.95875 20.001 5.85656 19.071 4.92901C18.1435 3.99902 17.0413 3.26151 15.8278 2.75885C14.6143 2.25619 13.3135 1.99831 12 2.00001C10.6865 1.99833 9.3857 2.25623 8.17222 2.75889C6.95875 3.26154 5.85656 3.99904 4.92901 4.92901C3.99904 5.85656 3.26154 6.95875 2.75889 8.17222C2.25623 9.3857 1.99833 10.6865 2.00001 12C1.99831 13.3135 2.25619 14.6143 2.75885 15.8278C3.26151 17.0413 3.99902 18.1435 4.92901 19.071C5.85656 20.001 6.95875 20.7385 8.17222 21.2411C9.3857 21.7438 10.6865 22.0017 12 22Z" stroke="#DA1C1C" stroke-width="2" stroke-linejoin="round"/>
                      <path fill-rule="evenodd" clip-rule="evenodd" d="M12 18.5C12.3315 18.5 12.6495 18.3683 12.8839 18.1339C13.1183 17.8995 13.25 17.5815 13.25 17.25C13.25 16.9185 13.1183 16.6005 12.8839 16.3661C12.6495 16.1317 12.3315 16 12 16C11.6685 16 11.3505 16.1317 11.1161 16.3661C10.8817 16.6005 10.75 16.9185 10.75 17.25C10.75 17.5815 10.8817 17.8995 11.1161 18.1339C11.3505 18.3683 11.6685 18.5 12 18.5Z" fill="#DA1C1C"/>
                      <path d="M12 6V14" stroke="#DA1C1C" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                    </svg>
                    ${message}
              <div/>`,
        position: 'top',
        background: '#f8d7da',
        showConfirmButton: false,
        showCloseButton: true,
        customClass: {
            closeButton: 'my-close-button-class'
        },
        backdrop: false,
        timer: 3000
    });
}

// Example starter JavaScript for disabling form submissions if there are invalid fields
(function () {
    'use strict'

    if (getCookie(".AspNetCore.Culture") === "c%3Duk%7Cuic%3Duk") {
        $("#kyr-lang").addClass("lang-link-active");
    } else {
        $("#rus-lang").addClass("lang-link-active");
    }

    // Fetch all the forms we want to apply custom Bootstrap validation styles to
    var forms = document.querySelectorAll('.needs-validation')

    // Loop over them and prevent submission
    Array.prototype.slice.call(forms)
        .forEach(function (form) {
            form.addEventListener('submit', function (event) {                
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                }

                form.classList.add('was-validated');
            }, false)
        })
})();

$(document).ready(function () {
    $('.phone-input').inputmask("+\\9\\96 (999) 99-99-99");
    
    // $('.email-input').on('input', function() {
    //     const email = $(this).val();
    //     const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    //    
    //     if (emailPattern.test(email)) {
    //         $(this).removeClass('invalid').addClass('valid');
    //     } else {
    //         $(this).removeClass('valid').addClass('invalid');
    //     }
    // });
    
    $(document).ready(function() {
        let today = new Date().toISOString().split('T')[0];
        $('#gns_date').attr('min', today);
    });
});

function showLoader() {
    $('#loader').show();
    $('#overlay').show();
}

function hideLoader() {
    $('#loader').hide();
    $('#overlay').hide();
}

/*
* 1 - Print
* 2 - Download
* 3 - Copy
*/
function broadCast(type) {
    switch (type) {
        case 1: {
            $(document).trigger('print-event');
            break;
        }
        case 2: {
            $(document).trigger('download-event');
            break;
        }
        case 3: {
            $(document).trigger('copy-event');
            break;
        }
    }
}

function downloadHtml(html) {
    let myElement = document.createElement("div");
    myElement.innerHTML = html;
    let container = document.createElement("div");
    container.style.display = "flex";
    container.style.justifyContent = "center";
    container.style.alignItems = "center";
    container.style.width = 600 + "px";
    container.appendChild(myElement);
    document.body.appendChild(container); // Append the container to the DOM
    html2canvas(container).then(function(canvas) {
        let dataURL = canvas.toDataURL();
        let link = document.createElement("a");
        link.download = "talon.png";
        link.href = dataURL;
        link.click();
        link.remove();
    });
    container.remove(); // Remove the container from the DOM
}

function printHtml(html) {
    let newWindow = window.open();
    newWindow.document.write('<html><head>');
    newWindow.document.write('<link rel="stylesheet" href="/css/site.css">');
    newWindow.document.write('<link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.min.css">');
    newWindow.document.write('<style>/* Add custom styles here */</style>');
    newWindow.document.write('</head><body>');
    newWindow.document.write(html);
    newWindow.document.write('</body></html>');

    // Delay printing using setTimeout
    setTimeout(function() {
        newWindow.print();
        newWindow.close();
    }, 2000);
}

function copyText(text) {
    navigator.clipboard.writeText(text)
        .then(function() {
            alert("Ваш идентификационный код сохранен!");
        })
        .catch(function(error) {
            alert(`Не получилось сохранить: ${error}`);
        });
}