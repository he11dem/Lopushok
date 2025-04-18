using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lopushok.DB
{
    partial class Product
    {
        public string TextMaterialList
        {
            get
            {
                return $"Материалы: {string.Join(",", Product_Materials.Select(x => x.Materials.NameMaterials))}";
            }
            set
            {

            }
        }


        public string TotalCost
        {
            get
            {
                // Проверяем, есть ли материалы в списке
                if (Product_Materials == null || !Product_Materials.Any())
                {
                    return "0.00 руб.";
                }

                // Вычисляем общую стоимость
                decimal total = (decimal)Product_Materials.Sum(x =>
                {
                    // Проверяем, что Materials и Cost не null
                    decimal cost = x.Materials?.Cost ?? 0; // Если Materials или Cost null, используем 0
                    return cost * x.NumberMaterials;
                });

                // Возвращаем отформатированную строку
                return $"{total:N2} руб.";
            }
            set
            {
            }
        }
    }
}
