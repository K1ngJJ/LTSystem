    function openCreateModal() {
    var createModal = new bootstrap.Modal(document.getElementById('createUserModal'));
    createModal.show();
}

    function openEditModal(id, username, email, role) {
        document.getElementById("editUserId").value = id;
    document.getElementById("editUsername").value = username;
    document.getElementById("editEmail").value = email;
    document.getElementById("editRole").value = role;

    var editModal = new bootstrap.Modal(document.getElementById('editUserModal'));
    editModal.show();
}

    function openDeleteModal(id, username) {
        document.getElementById("deleteUserId").value = id;
    document.getElementById("deleteUsername").innerText = username;

    var deleteModal = new bootstrap.Modal(document.getElementById('deleteUserModal'));
    deleteModal.show();
}

