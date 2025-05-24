// Функции для создания различных типов графиков

function createHalsteadChart(containerId, data) {
    const ctx = document.getElementById(containerId).getContext('2d');
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['Объем программы', 'Сложность', 'Усилия', 'Время', 'Ошибки'],
            datasets: [{
                label: 'Метрики Холстеда',
                data: [data.volume, data.difficulty, data.effort, data.time, data.bugs],
                backgroundColor: [
                    'rgba(54, 162, 235, 0.5)',
                    'rgba(255, 99, 132, 0.5)',
                    'rgba(75, 192, 192, 0.5)',
                    'rgba(255, 206, 86, 0.5)',
                    'rgba(153, 102, 255, 0.5)'
                ],
                borderColor: [
                    'rgb(54, 162, 235)',
                    'rgb(255, 99, 132)',
                    'rgb(75, 192, 192)',
                    'rgb(255, 206, 86)',
                    'rgb(153, 102, 255)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}

function createGilbChart(containerId, data) {
    const ctx = document.getElementById(containerId).getContext('2d');
    
    // Нормализуем значения для лучшей визуализации
    const maintainabilityIndex = Math.min(data.maintainabilityIndex, 100);
    const codeQuality = Math.min(data.codeQuality, 100);
    
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ['Индекс поддерживаемости', 'Качество кода'],
            datasets: [{
                label: 'Значение',
                data: [maintainabilityIndex, codeQuality],
                backgroundColor: [
                    'rgba(75, 192, 192, 0.5)',
                    'rgba(54, 162, 235, 0.5)'
                ],
                borderColor: [
                    'rgb(75, 192, 192)',
                    'rgb(54, 162, 235)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const value = context.raw;
                            let label = context.dataset.label || '';
                            if (label) {
                                label += ': ';
                            }
                            label += value.toFixed(2) + '%';
                            return label;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    max: 100,
                    title: {
                        display: true,
                        text: 'Процент'
                    }
                }
            }
        }
    });
}

function createChepinChart(containerId, data) {
    const ctx = document.getElementById(containerId).getContext('2d');
    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: ['Вводимые (P)', 'Модифицируемые (M)', 'Управляющие (C)', 'Неиспользуемые (U)'],
            datasets: [{
                data: [
                    data.inputVariables,
                    data.modifiedVariables,
                    data.controlVariables,
                    data.unusedVariables
                ],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.5)',
                    'rgba(54, 162, 235, 0.5)',
                    'rgba(255, 206, 86, 0.5)',
                    'rgba(75, 192, 192, 0.5)'
                ],
                borderColor: [
                    'rgb(255, 99, 132)',
                    'rgb(54, 162, 235)',
                    'rgb(255, 206, 86)',
                    'rgb(75, 192, 192)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: true,
                    text: 'Распределение переменных по метрике Чепина'
                }
            }
        }
    });
}

function createErrorPredictionChart(containerId, data) {
    const ctx = document.getElementById(containerId).getContext('2d');
    
    // Создаем градиент для области под линией
    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, 'rgba(75, 192, 192, 0.5)');
    gradient.addColorStop(1, 'rgba(75, 192, 192, 0)');
    
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.volumes,
            datasets: [{
                label: 'Предсказанное количество ошибок',
                data: data.errors,
                fill: true,
                backgroundColor: gradient,
                borderColor: 'rgb(75, 192, 192)',
                tension: 0.4,
                pointRadius: 4,
                pointHoverRadius: 6
            }]
        },
        options: {
            responsive: true,
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const value = context.raw;
                            return `Ошибок: ${value.toFixed(2)}`;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Количество ошибок'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Объем программы'
                    }
                }
            }
        }
    });
} 