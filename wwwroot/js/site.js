// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', () => {
    const formatMoney = value => `${Math.round(value).toLocaleString('vi-VN')} đ`;
    const updateCartTotals = () => {
        let total = 0;
        document.querySelectorAll('.cart-row').forEach(row => {
            const price = Number(row.querySelector('.unit-price')?.dataset.price || 0);
            const quantityInput = row.querySelector('input[name="quantity"]');
            const quantity = Math.max(1, Math.min(Number(quantityInput?.max || 1), Number(quantityInput?.value || 1)));
            if (quantityInput) quantityInput.value = quantity;
            const lineTotal = price * quantity;
            const lineElement = row.querySelector('.line-total');
            if (lineElement) { lineElement.dataset.total = lineTotal; lineElement.textContent = formatMoney(lineTotal); }
            total += lineTotal;
        });
        const totalElement = document.querySelector('#cart-total');
        if (totalElement) { totalElement.dataset.total = total; totalElement.textContent = formatMoney(total); }
    };
    document.querySelectorAll('.quantity-form input[name="quantity"]').forEach(input => {
        input.addEventListener('input', updateCartTotals);
        input.addEventListener('change', () => { updateCartTotals(); input.form?.requestSubmit(); });
    });
    updateCartTotals();
});
