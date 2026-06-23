function onAddCopySuccess(row) {
	showSuccessMessage();
	$('#Modal').modal('hide');

	$('#copies').prepend(row);
	KTMenu.createInstances();

	var count = $('#copiescount');
	var newcount = parseInt(count.text()) + 1;
	count.text(newcount);

	$('.js-alert').addClass('d-none');
	$('#copies').removeClass('d-none');

}
function onEditCopySuccess(row) {
	showSuccessMessage();
	$('#modal').modal('hide');

	$(updatedRow).replaceWith(row);
	KTMenu.createInstances();
}