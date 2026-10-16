/* Thông báo lỗi tiếng Việt cho jQuery Validation */
$.extend($.validator.messages, {
    required: "Trường này không được để trống.",
    remote: "Vui lòng sửa lại trường này.",
    email: "Địa chỉ email không hợp lệ (ví dụ: ten@gmail.com).",
    url: "Đường dẫn không hợp lệ.",
    date: "Ngày không hợp lệ.",
    dateISO: "Ngày không hợp lệ (định dạng ISO).",
    number: "Vui lòng nhập một số hợp lệ.",
    digits: "Chỉ được nhập chữ số.",
    creditcard: "Số thẻ tín dụng không hợp lệ.",
    equalTo: "Vui lòng nhập lại giá trị giống ở trên.",
    maxlength: $.validator.format("Vui lòng nhập tối đa {0} ký tự."),
    minlength: $.validator.format("Vui lòng nhập ít nhất {0} ký tự."),
    rangelength: $.validator.format("Vui lòng nhập từ {0} đến {1} ký tự."),
    range: $.validator.format("Giá trị phải nằm trong khoảng {0} đến {1}."),
    max: $.validator.format("Giá trị phải nhỏ hơn {0}."),
    min: $.validator.format("Giá trị phải lớn hơn {0}.")
});