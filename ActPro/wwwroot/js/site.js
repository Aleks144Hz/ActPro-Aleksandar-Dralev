toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "timeOut": "3000"
};

$(document).on('click', '.confirm-action', function (e)
{
    e.preventDefault();

    const form = $(this).closest('form');
    const message = $(this).data('message') || "";

    Swal.fire({
        title: confirmDialogTitle || "Are you sure?",
        text: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#198754',
        cancelButtonColor: '#d33',
        confirmButtonText: confirmYes || 'Yes!',
        cancelButtonText: confirmCancel || 'Cancel'
    }).then((result) => {
        if (result.isConfirmed){
            form.submit(); 
        }
    });
});
