using System;
using System.Collections.Generic;

namespace MyManual.Helpers
{
	public static class WorkingDayCalculator
	{
		// �Ի��Ϻ��� ���ñ��� ������ ���
		public static int GetWorkingDay(DateTime joinDate)
		{
			DateTime today = DateTime.Today;
			int workingDays = 0;

			for (DateTime date = joinDate; date <= today; date = date.AddDays(1))
			{
				// �ָ� ����
				if (date.DayOfWeek != DayOfWeek.Saturday &&
					date.DayOfWeek != DayOfWeek.Sunday)
				{
					workingDays++;
				}
			}

			return workingDays;
		}

		// ������ ���� (����)
		public static bool IsHoliday(DateTime date)
		{
			// 2026�� �ѱ� ������ ����
			var holidays = new List<DateTime>
		{
			new DateTime(2026, 1, 1),   // ����
            new DateTime(2026, 3, 1),   // ������
            new DateTime(2026, 5, 5),   // ��̳�
            new DateTime(2026, 6, 6),   // ������
            new DateTime(2026, 8, 15),  // ������
            new DateTime(2026, 10, 3),  // ��õ��
            new DateTime(2026, 10, 9),  // �ѱ۳�
            new DateTime(2026, 12, 25), // ũ��������
            // ����, �߼��� �ų� �����̶� ���� ��� �ʿ�
        };

			return holidays.Contains(date.Date);
		}

		// ������ ���� ������ ���
		public static int GetWorkingDayExcludingHolidays(DateTime joinDate)
		{
			DateTime today = DateTime.Today;
			int workingDays = 0;

			for (DateTime date = joinDate; date <= today; date = date.AddDays(1))
			{
				if (date.DayOfWeek != DayOfWeek.Saturday &&
					date.DayOfWeek != DayOfWeek.Sunday &&
					!IsHoliday(date))
				{
					workingDays++;
				}
			}

			return workingDays;
		}
	}
}