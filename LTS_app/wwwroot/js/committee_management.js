function openCreateModal() {
    var createModal = new bootstrap.Modal(document.getElementById('createCommitteeModal'));
    createModal.show();
}

function openEditModal(id, name, description) {
    document.getElementById("editCommitteeId").value = id;
    document.getElementById("editCommitteeName").value = name;
    document.getElementById("editCommitteeDescription").value = description;

    // Update form action dynamically
    document.getElementById("editCommitteeForm").action = "/Committee/Edit/" + id;

    var editModal = new bootstrap.Modal(document.getElementById('editCommitteeModal'));
    editModal.show();
}

function openDeleteModal(id, name) {
    if (!id) {
        alert("Invalid ID!");
        return;
    }

    document.getElementById("deleteCommitteeId").value = id;
    document.getElementById("deleteCommitteeName").innerText = name;

    var deleteModal = new bootstrap.Modal(document.getElementById('deleteCommitteeModal'));
    deleteModal.show();
}

    function setDeleteCommittee(id, name) {
        document.getElementById("deleteCommitteeId").value = id;
        document.getElementById("deleteCommitteeName").innerText = name;
    }

    $(document).ready(function () {
        $("#confirmDelete").click(function () {
            var id = $("#deleteCommitteeId").val();
            $.ajax({
                url: "/Committee/Delete",
                type: "POST",
                data: { id: id, __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() },
                success: function (response) {
                    if (response.success) {
                        $("#deleteCommitteeModal").modal("hide");
                        location.reload(); // Refresh page after deletion
                    } else {
                        alert("Error: " + response.message);
                    }
                },
                error: function () {
                    alert("Something went wrong!");
                }
            });
        });
    });