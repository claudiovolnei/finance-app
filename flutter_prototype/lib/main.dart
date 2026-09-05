import 'package:flutter/material.dart';

void main() => runApp(const FinanceApp());

class FinanceApp extends StatelessWidget {
  const FinanceApp({super.key});
  @override
  Widget build(BuildContext context) => MaterialApp(
    debugShowCheckedModeBanner: false,
    title: 'Finance',
    theme: ThemeData(useMaterial3: true, colorSchemeSeed: Colors.indigo, scaffoldBackgroundColor: const Color(0xFFF6F7FB)),
    home: const DashboardPage(),
  );
}

class Transaction {
  final String title, category, date;
  final double amount;
  final bool income;
  const Transaction({required this.title, required this.category, required this.amount, required this.income, required this.date});
}

class MockFinanceApi {
  static Future<Map<String, dynamic>> getDashboard() async {
    await Future.delayed(const Duration(milliseconds: 500));
    return {
      'balance': 12450.80, 'income': 9850.00, 'expenses': 4320.50, 'credit_card': 1870.30,
      'transactions': const [
        Transaction(title: 'Salário', category: 'Renda', amount: 9850, income: true, date: 'Hoje'),
        Transaction(title: 'Supermercado', category: 'Alimentação', amount: 386.40, income: false, date: 'Hoje'),
        Transaction(title: 'Netflix', category: 'Assinaturas', amount: 55.90, income: false, date: 'Ontem'),
        Transaction(title: 'Combustível', category: 'Transporte', amount: 250, income: false, date: '02/09'),
        Transaction(title: 'Freelance', category: 'Renda extra', amount: 1200, income: true, date: '01/09'),
      ],
    };
  }
}

class DashboardPage extends StatefulWidget {
  const DashboardPage({super.key});
  @override State<DashboardPage> createState() => _DashboardPageState();
}

class _DashboardPageState extends State<DashboardPage> {
  Map<String, dynamic>? data;
  @override void initState() { super.initState(); _load(); }
  Future<void> _load() async { final result = await MockFinanceApi.getDashboard(); if (mounted) setState(() => data = result); }
  String money(double v) => 'R\$ ${v.toStringAsFixed(2).replaceAll('.', ',')}';

  @override
  Widget build(BuildContext context) {
    if (data == null) return const Scaffold(body: Center(child: CircularProgressIndicator()));
    final tx = data!['transactions'] as List<Transaction>;
    return Scaffold(
      appBar: AppBar(title: const Text('Finance'), actions: [IconButton(onPressed: _load, icon: const Icon(Icons.refresh))]),
      floatingActionButton: FloatingActionButton.extended(onPressed: () => _add(context), icon: const Icon(Icons.add), label: const Text('Lançamento')),
      body: RefreshIndicator(
        onRefresh: _load,
        child: LayoutBuilder(builder: (context, c) {
          final wide = c.maxWidth >= 800;
          return SingleChildScrollView(
            physics: const AlwaysScrollableScrollPhysics(), padding: const EdgeInsets.all(20),
            child: Center(child: ConstrainedBox(constraints: const BoxConstraints(maxWidth: 1200), child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text('Visão geral', style: Theme.of(context).textTheme.headlineMedium?.copyWith(fontWeight: FontWeight.bold)),
              const SizedBox(height: 18),
              GridView.count(crossAxisCount: wide ? 4 : 2, shrinkWrap: true, physics: const NeverScrollableScrollPhysics(), mainAxisSpacing: 12, crossAxisSpacing: 12, childAspectRatio: wide ? 1.8 : 1.35,
                children: [_card('Saldo', money(data!['balance']), Icons.account_balance_wallet), _card('Receitas', money(data!['income']), Icons.trending_up), _card('Despesas', money(data!['expenses']), Icons.trending_down), _card('Cartão', money(data!['credit_card']), Icons.credit_card)]),
              const SizedBox(height: 24), const Text('Gastos por categoria', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold)), const SizedBox(height: 12), _chart(),
              const SizedBox(height: 24), const Text('Últimas transações', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
              Card(child: Column(children: tx.map((t) => ListTile(leading: CircleAvatar(child: Icon(t.income ? Icons.arrow_downward : Icons.arrow_upward)), title: Text(t.title, style: const TextStyle(fontWeight: FontWeight.w600)), subtitle: Text('${t.category} • ${t.date}'), trailing: Text('${t.income ? '+' : '-'} ${money(t.amount)}', style: TextStyle(fontWeight: FontWeight.bold, color: t.income ? Colors.green : Colors.red))).toList())),
              const SizedBox(height: 90),
            ]))),
          );
        }),
      ),
    );
  }

  Widget _card(String title, String value, IconData icon) => Card(child: Padding(padding: const EdgeInsets.all(16), child: Column(crossAxisAlignment: CrossAxisAlignment.start, mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [Icon(icon), Text(title), FittedBox(child: Text(value, style: const TextStyle(fontSize: 21, fontWeight: FontWeight.bold)))])));
  Widget _chart() { final items = [('Alimentação', .72), ('Transporte', .48), ('Moradia', .35), ('Assinaturas', .22)]; return Card(child: Padding(padding: const EdgeInsets.all(18), child: Column(children: items.map((i) => Padding(padding: const EdgeInsets.symmetric(vertical: 8), child: Row(children: [SizedBox(width: 105, child: Text(i.$1)), Expanded(child: LinearProgressIndicator(value: i.$2, minHeight: 10, borderRadius: BorderRadius.circular(10))), const SizedBox(width: 12), Text('${(i.$2 * 100).round()}%')])).toList()))); }
  void _add(BuildContext context) => showModalBottomSheet(context: context, isScrollControlled: true, builder: (_) => const AddTransactionSheet());
}

class AddTransactionSheet extends StatefulWidget { const AddTransactionSheet({super.key}); @override State<AddTransactionSheet> createState() => _AddTransactionSheetState(); }
class _AddTransactionSheetState extends State<AddTransactionSheet> {
  final title = TextEditingController(), amount = TextEditingController(); bool income = false;
  @override Widget build(BuildContext context) => Padding(padding: EdgeInsets.only(left: 20, right: 20, top: 20, bottom: MediaQuery.of(context).viewInsets.bottom + 20), child: Column(mainAxisSize: MainAxisSize.min, children: [const Text('Novo lançamento', style: TextStyle(fontSize: 21, fontWeight: FontWeight.bold)), const SizedBox(height: 16), TextField(controller: title, decoration: const InputDecoration(labelText: 'Descrição', border: OutlineInputBorder())), const SizedBox(height: 12), TextField(controller: amount, keyboardType: const TextInputType.numberWithOptions(decimal: true), decoration: const InputDecoration(labelText: 'Valor', border: OutlineInputBorder())), SwitchListTile(title: const Text('É uma receita'), value: income, onChanged: (v) => setState(() => income = v)), SizedBox(width: double.infinity, child: FilledButton(onPressed: () => Navigator.pop(context), child: const Text('Salvar (mock)')))]));
}
